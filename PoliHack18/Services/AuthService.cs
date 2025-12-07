using System.Data;
using System.Diagnostics;

namespace PoliHack18.Services
{
    public class AuthService
    {
        public enum LoginResult
        {
            Success,
            InvalidCredentials,
            DatabaseError,
            UserNotFound
        }

        public LoginResult LoginUser(string email, string password, out DataRow? userData)
        {
            userData = null;
            string hashedPassword = string.Empty;

            try
            {

                string query = "SELECT * FROM users WHERE email = @EmailParam";

                var parameters = new Dictionary<string, object?>
                {
                    { "EmailParam", email }
                };

                DataTable dt = Database.ExecutaQuery(query, parameters);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return LoginResult.UserNotFound;
                }


                userData = dt.Rows[0];

                hashedPassword = userData["password"] as string;

                if (string.IsNullOrEmpty(hashedPassword))
                {
                    Debug.WriteLine($"Error: User {email} found, but 'password' hash is missing.");
                    return LoginResult.DatabaseError;
                }

                if (Password.VerifyPassword(password, hashedPassword))
                {
                    Guid userId;
                    if (userData["id"] is Guid g) userId = g;
                    else userId = Guid.Parse(userData["id"].ToString());

                    UserSession.Login(userId);
                    return LoginResult.Success;
                }
                else
                {
                    return LoginResult.InvalidCredentials;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database or service error during login: {ex.Message}");
                return LoginResult.DatabaseError;
            }
        }
    }
}