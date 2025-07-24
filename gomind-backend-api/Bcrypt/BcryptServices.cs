namespace gomind_backend_api.Bcrypt
{
    public class BcryptServices
    {
        public string HashPassword(string password)
        {
            // El segundo parámetro es el factor de trabajo (log_rounds).
            // Un valor más alto hace el hash más lento y seguro.
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
