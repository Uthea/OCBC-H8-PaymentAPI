using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PaymentAPI.Configuration;
using PaymentAPI.Models.DTOs.Requests;

namespace PaymentAPI.Services
{
    public interface IAuthManagementService
    {
        Task<IdentityUser> FindUserByEmail(string email);
        Task<IdentityResult> CreateNewUser(UserRegistrationDto user);
        Task<bool> CheckPassword(IdentityUser user, string password);
        Task<AuthResult> GenerateJwtToken(IdentityUser user);
        Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest);
    }
}