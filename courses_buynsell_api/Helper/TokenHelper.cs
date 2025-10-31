using System.Security.Cryptography;

namespace courses_buynsell_api.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}