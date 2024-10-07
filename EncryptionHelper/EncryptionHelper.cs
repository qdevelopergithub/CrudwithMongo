using System.Text;
using System.Security.Cryptography;

namespace CrudWithMongoDB.EncryptionHelper
{
    public class EncryptionHelper
    {
        public static string Encrypt(string plainText, string key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(key);
                    aes.IV = new byte[16];

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(plainText);
                        return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception to gather more context
                Console.WriteLine($"Error during encryption: {ex.Message}");
                throw;
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(key);
                    aes.IV = new byte[16];

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] buffer = Convert.FromBase64String(cipherText);
                        return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception to gather more context
                Console.WriteLine($"Error during decryption: {ex.Message}");
                throw;
            }
        }
    }
}
