using System.Data;
using System.Diagnostics;
namespace PoliHack18.Services
{
    public class AuthService
    {
        // Enum for clear status reporting
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
                // 1. Define the secure query to fetch the user's data based on email.
                // We fetch everything, but mainly need the 'password_hash' and the user's unique ID.
                string query = "SELECT * FROM users WHERE email = @EmailParam";

                // 2. Prepare the parameters dictionary for security.
                var parameters = new Dictionary<string, object?>
                {
                    { "EmailParam", email }
                };

                // 3. Execute the query using the secure method.
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