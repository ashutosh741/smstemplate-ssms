using System;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Methods
{
    public class VerifyPasswordService
    {
        // Method to verify password
        public bool VerifyPassword(string inputPassword, string storedPassword, string storedSalt)
        {
            Console.WriteLine("Passwords for comparison:");
            Console.WriteLine($"Input Password: {inputPassword}");
            Console.WriteLine($"Stored Password: {storedPassword}");

            // Step 1: Generate hash of input password using stored salt
            string hashedInputPassword = HashPassword(inputPassword, storedSalt);

            // Step 2: Compare hashed input password with stored password hash
            return hashedInputPassword == storedPassword;
        }

        // Method to generate salt
        private string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Method to hash password with salt
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = salt + password;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
