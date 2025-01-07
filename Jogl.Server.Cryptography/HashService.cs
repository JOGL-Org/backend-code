
using System.Security.Cryptography;
using System.Text;

namespace Jogl.Server.Cryptography
{
    public class HashService : IHashService
    {
        public string ComputeHash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array
                var inputBytes = Encoding.UTF8.GetBytes(data);

                // Compute the hash
                var hashBytes = sha256.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
