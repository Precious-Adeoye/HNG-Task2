using System.Security.Cryptography;
using System.Text;

namespace HNG_Task2.Utility
{
    public class HashHelper
    {
        public static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
