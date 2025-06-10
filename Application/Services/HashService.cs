using System.Text;
using System.Security.Cryptography;

namespace simple_rank_backend.Application.Services
{
    public static class HashService
    {
        public static string GenerateRobustHash(string input)
        {
            input = string.Concat(input.ToLower().Where(character => !char.IsWhiteSpace(character)));
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                Array.Resize(ref hashBytes, 16);
                return new Guid(hashBytes).ToString();
            }
        }

        public static long GenerateLongHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                long value = BitConverter.ToInt64(hashBytes, 0);
                return value;
            }
        }
    }
}
