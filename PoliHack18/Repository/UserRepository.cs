using PoliHack18.Models;
using PoliHack18.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliHack18.Repository
{
    public class UserRepository
    {
        public async Task AddUser(User user)
        {
            string query =
            "INSERT INTO users ( \"email\", \"password\", \"name\", age, \"phone_number\") " +
            "VALUES (@email, @password, @name, @age, @telephone_number)";
            string hashpassword = Password.HashPassword(user.Password);
            var parameters = new Dictionary<string, object>
                {
                    {"@email",user.Email},
                    {"@password",hashpassword },
                    {"@name",user.Name},
                    {"@age",user.Age},
                    {"@telephone_number",user.PhoneNumber }
                };
            await Task.Run(() => Database.ExecutaNonQuery(query, parameters));
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            string query =
                "SELECT COUNT(*) FROM users WHERE email = @email;";

            var parameters = new Dictionary<string, object?>
            {
                { "@email", email }
            };

            
            long count = await Task.Run(() =>
            {
                
                return Database.ExecutaScalar<long>(query, parameters);
            });

            return count > 0;
        }
    }
}
