using SHA3.Net;
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
            return await Task.Run(() =>
            {
                const string saltPrefix = "aP.7QQ!?XnbGGk94";
                byte[] salt = SHA256.HashData(Encoding.UTF8.GetBytes(password + saltPrefix));

                Span<byte> passwordSalt = stackalloc byte[salt.Length + Encoding.UTF8.GetByteCount(password)];
                salt.CopyTo(passwordSalt);
                Encoding.UTF8.GetBytes(password, passwordSalt[salt.Length..]);

                const int iterations = 1_000_000;
                byte[] hash = passwordSalt.ToArray();
                for (int i = 0; i < iterations; i++)
                    hash = Sha3.Sha3384().ComputeHash(hash);

                byte[] key = new byte[32];
                Buffer.BlockCopy(hash, 0, key, 0, 32);
                byte[] iv = Sha3.Sha3256().ComputeHash(Encoding.UTF8.GetBytes(password + ":iv"))[..16];

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
