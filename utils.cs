using System;
using System.Security.Cryptography;

namespace WebApplication1.utils // Note: lowercase 'utils' here
{
    public static class Utils
    {
        public static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string salt)
        {
            // Combine password and salt
            string combinedPassword = $"{salt}:{password}";

            // Create SHA256 hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        internal static string HashPassword(object? password, string salt)
        {
            string combinedPassword = $"{salt}:{password}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
