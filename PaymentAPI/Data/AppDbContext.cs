using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;

namespace PaymentAPI.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        
        public virtual DbSet<PaymentDetail> PaymentDetails { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}