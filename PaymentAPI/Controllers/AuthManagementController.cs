using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentAPI.Configuration;
using PaymentAPI.Data;
using PaymentAPI.Models;
using PaymentAPI.Models.DTOs.Requests;
using PaymentAPI.Models.DTOs.Responses;

namespace PaymentAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthManagementController : ControllerBase
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthManagementController(
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParameters,
            AppDbContext appDbContext
        )
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _appDbContext = appDbContext;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (ModelState.IsValid) 
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if(existingUser != null)
                {
                    return BadRequest(new RegistrationResponse(){
                        Errors = new List<string>() {
                            "Email already in use"
                        },
                        Success = false
                    });
                }

                var newUser = new IdentityUser() {Email = user.Email, UserName = user.Username};
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);

                if(isCreated.Succeeded)
                {
                    //var jwtToken = await GenerateJwtToken(newUser);
                    return new JsonResult("Register success !!") {StatusCode = 200};
                }
                else
                {
                    return BadRequest(new RegistrationResponse() {
                        Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }

        
            return BadRequest(new RegistrationResponse() {
                Errors = new List<string>() {
                    "Invalid payload"
                },
                Success = false
            });

        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login ([FromBody] UserLoginRequest user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if(existingUser == null)
                {
                    return BadRequest(new RegistrationResponse() {
                        Errors = new List<string>() {
                            "Invalid login request"
                        },

                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if(!isCorrect)
                {
                    return BadRequest(new RegistrationResponse(){
                        Errors = new List<string>() {
                            "Invalid login request"
                        },
                        Success = false
                    });
                }


                var jwtToken = await GenerateJwtToken(existingUser);

                return Ok(jwtToken);
            }

            return BadRequest(new RegistrationResponse() {
                Errors = new List<string>() {
                    "Invalid payload"
                },
                Success = false
            });
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var expireTime = DateTime.UtcNow.AddMinutes(30);

            var expireDate = DateTime.UtcNow.AddHours(2);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new [] {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),

                Expires = expireTime,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevorked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = expireDate,
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _appDbContext.RefreshTokens.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();
            
            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await VerifyAndGenerateToken(tokenRequest);

                if (result == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>()
                        {
                            "Invalid tokens"
                        },
                        Success = false
                    });
                }

                return Ok(result);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Success = false
            });
            
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var tokenInVerification = ValidateJwtTokenAlgorithm(tokenRequest.Token);

            if (tokenInVerification == null)
            {
                return null;
            }

            var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(
                x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDate = UnixTimeStampToDateTime(utcExpiryDate) ;

            if (expiryDate > DateTime.UtcNow)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Token has not expired yet"
                    }
                };
            }

            var storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(
                x => x.Token == tokenRequest.RefreshToken);

            if (storedToken == null)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Token does not exist"
                    }
                };
            }

            if (storedToken.IsRevorked)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Token has been revoked"
                    }
                };
            }

            var jti = tokenInVerification.Claims.FirstOrDefault(
                x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedToken.JwtId != jti)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Token doesn't match"
                    }
                };
                
            }
            if (storedToken.IsUsed)
             {
                 return new AuthResult()
                 {
                     Success = false,
                     Errors = new List<string>()
                     {
                         "Token has been used"
                     }
                 };
             }
            
            storedToken.IsUsed = true;
            _appDbContext.RefreshTokens.Update(storedToken);
            await _appDbContext.SaveChangesAsync();

            var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
            return await GenerateJwtToken(dbUser);
        }

        private ClaimsPrincipal ValidateJwtTokenAlgorithm(string token)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var tokenInVerification = jwtTokenHandler.ValidateToken(
                                    token,
                                    _tokenValidationParameters,
                                    out var validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tokenInVerification;
                }
                else
                {
                    return null;
                }
            }
            catch 
            {
                return null;
            } 
        }
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }


        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)]).ToArray());
        }


    }
}