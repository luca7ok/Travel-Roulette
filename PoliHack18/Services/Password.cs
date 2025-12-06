using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PoliHack18.Services
{
    public class Password
    {
        private const int CostFactor = 12;
        public static string HashPassword(string password)
        {
       
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: CostFactor);

            return hashedPassword;
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
