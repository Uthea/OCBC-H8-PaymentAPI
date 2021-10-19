using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentAPI.Configuration;
using PaymentAPI.Data;
using PaymentAPI.Models;
using PaymentAPI.Models.DTOs.Requests;

namespace PaymentAPI.Services
{
    public class AuthManagementService : IAuthManagementService
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly string _jwtSecret;

        public AuthManagementService(TokenValidationParameters tokenValidationParameters, AppDbContext appDbContext, UserManager<IdentityUser> userManager, string jwtSecret)
        {
            _tokenValidationParameters = tokenValidationParameters;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _jwtSecret = jwtSecret;
        }

        public async Task<IdentityUser> FindUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateNewUser(UserRegistrationDto user)
        {
            
            var newUser = new IdentityUser() {Email = user.Email, UserName = user.Username};
            return await _userManager.CreateAsync(newUser, user.Password);
            
        }
        public async Task<bool> CheckPassword(IdentityUser user, string password)
        {
           return await _userManager.CheckPasswordAsync(user, password);
        }
        public async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
             var jwtTokenHandler = new JwtSecurityTokenHandler();

             var key = Encoding.ASCII.GetBytes(_jwtSecret);

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
        
        public async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
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
        private string RandomString(int length)
        {
             var random = new Random();
             var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
 
             return new string(Enumerable.Repeat(chars, length)
                 .Select(x => x[random.Next(x.Length)]).ToArray());
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
    }


}