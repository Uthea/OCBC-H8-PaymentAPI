using System;
using Npgsql;

namespace PaymentAPI.Configuration
{
    public class EnvirontmentVar
    {
        public static string PostgreDatabaseConnection()
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            
            return builder.ToString();
        }

        public static string GetJwtSecret()
        {
            return Environment.GetEnvironmentVariable("SECRET");
        }
        
        public static string GetCorsUrl()
        {
            return Environment.GetEnvironmentVariable("FRONT_END_URL");
        } 
    }
}