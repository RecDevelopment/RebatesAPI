using System.Security.Cryptography;
using System.Text;

namespace RebatesAPI.Utilities
{
    public static class EncryptionOption
    {
        public static string getHash(string input)
        {
            string result = null;

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashBytes = sha512.ComputeHash(inputBytes);
                result = BitConverter.ToString(hashBytes).Replace("-", "");
            }

            return result;
        }

    }
}
