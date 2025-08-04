using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NC.SalaryCalculator.Utils
{
    public class StringEncryptionService
    {
        private int Keysize = 256;
        private string DefaultPassPhrase = "gsKnGZ041HLL4IM8";
        private byte[] InitVectorBytes = Encoding.ASCII.GetBytes("jkE49230Tf093b42");
        private byte[] DefaultSalt = Encoding.ASCII.GetBytes("hgt!16kl");

        public StringEncryptionService()
        {
        }

        public virtual string? Encrypt(string? plainText, string? passPhrase = null, byte[]? salt = null)
        {
            if (plainText == null)
            {
                return null;
            }

            if (passPhrase == null)
            {
                passPhrase = DefaultPassPhrase;
            }

            if (salt == null)
            {
                salt = DefaultSalt;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            using Rfc2898DeriveBytes rfc2898DeriveBytes = new(passPhrase, salt);
            byte[] bytes2 = rfc2898DeriveBytes.GetBytes(Keysize / 8);
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            using ICryptoTransform transform = aes.CreateEncryptor(bytes2, InitVectorBytes);
            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public virtual string? Decrypt(string? cipherText, string? passPhrase = null, byte[]? salt = null)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }

            if (passPhrase == null)
            {
                passPhrase = DefaultPassPhrase;
            }

            if (salt == null)
            {
                salt = DefaultSalt;
            }

            byte[] array = Convert.FromBase64String(cipherText);
            using Rfc2898DeriveBytes rfc2898DeriveBytes = new(passPhrase, salt);
            byte[] bytes = rfc2898DeriveBytes.GetBytes(Keysize / 8);
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            using ICryptoTransform transform = aes.CreateDecryptor(bytes, InitVectorBytes);
            using MemoryStream stream = new MemoryStream(array);
            using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
            byte[] array2 = new byte[array.Length];
            int i;
            int num;
            for (i = 0; i < array.Length; i += num)
            {
                byte[] array3 = new byte[array.Length];
                num = cryptoStream.Read(array3, 0, array3.Length);
                if (num == 0)
                {
                    break;
                }

                for (int j = 0; j < num; j++)
                {
                    array2[j + i] = array3[j];
                }
            }

            return Encoding.UTF8.GetString(array2, 0, i);
        }
    }
}
