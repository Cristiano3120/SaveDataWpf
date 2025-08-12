using Konscious.Security.Cryptography;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SaveDataWpf.Helper
{
    internal sealed class CryptoHelper
    {
        private static async Task<(byte[] key, byte[] iv)> GetKeyAndIVAsync(string password)
        {
            return await Task.Run(async() =>
            {
                byte[] salt = await FileSystemHelper.ReadSaltAsync();

                int gigabyte = 1000 * 1024; //1GB
                Argon2id argon2 = new(Encoding.UTF8.GetBytes(password))
                {
                    MemorySize = gigabyte, 
                    Iterations = 5,
                    DegreeOfParallelism = 1,
                    Salt = salt
                };

                byte[] derived = argon2.GetBytes(48);

                byte[] key = new byte[32];
                byte[] iv = new byte[16];

                Buffer.BlockCopy(derived, 0, key, 0, 32);
                Buffer.BlockCopy(derived, 32, iv, 0, 16);

                return (key, iv);
            });
        }

        public static async Task<string> EncryptAsync(string plainText, string password)
        {
            (byte[] key, byte[] iv) = await GetKeyAndIVAsync(password);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                await sw.WriteAsync(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }

        public static async Task<string?> DecryptAsync(string cipherText, string password, bool disableErrorMessage = false)
        {
            try
            {
                (byte[] key, byte[] iv) = await GetKeyAndIVAsync(password);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                byte[] bytes = Convert.FromBase64String(cipherText);

                using var ms = new MemoryStream(bytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return await sr.ReadToEndAsync();
            }
            catch (Exception)
            {
                if (!disableErrorMessage)
                    MessageBox.Show("Wrong password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
