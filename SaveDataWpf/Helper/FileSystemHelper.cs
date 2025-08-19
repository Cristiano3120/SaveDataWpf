using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SaveDataWpf.Helper
{
    internal sealed class FileSystemHelper
    {
        private const string _filename = "SavedData.json";
        private const string _saltFilename = "Salt.bin";

        public static async Task<byte[]> ReadSaltAsync()
        {
            byte saltLength = 32;
            Memory<byte> buffer = new byte[saltLength];

            MakeSureSaltIsHealthy();
            using (FileStream fileStream = File.OpenRead(GetDynamicPath(_saltFilename)))
            {
                try
                {
                    await fileStream.ReadExactlyAsync(buffer);
                }
                catch { }
            }

            SearchValues<byte> searchValues = SearchValues.Create(0x00); 
            if (buffer.Length != saltLength || !buffer.Span.ContainsAnyExcept(searchValues))
            {
                return await WriteSaltAsync(saltLength);
            }

            return buffer.ToArray();
        }

        public static async Task<byte[]> WriteSaltAsync(byte saltLength)
        {
            byte[] salt = new byte[saltLength];
            RandomNumberGenerator.Fill(salt);

            using (FileStream fileStream = File.OpenWrite(GetDynamicPath(_saltFilename)))
            {
                await fileStream.WriteAsync(salt);
            }

            return salt;
        }

        private static void MakeSureSaltIsHealthy()
        {
            string saltFilePath = GetDynamicPath(_saltFilename);
            if (!File.Exists(saltFilePath))
            {
                File.Create(saltFilePath).Close();
            }
        }

        public static string GetDynamicPath(string relativePath)
        {
#if DEBUG
            string projectBasePath = AppContext.BaseDirectory;
            int binIndex = projectBasePath.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

            if (binIndex == -1)
            {
                throw new Exception("Could not determine project base path!");
            }

            projectBasePath = projectBasePath[..binIndex];
            return Path.Combine(projectBasePath, relativePath);

#else
            string currentDirectory = Directory.GetCurrentDirectory();
            return Path.Combine(currentDirectory, relativePath);
#endif
        }

        public static void CreateJsonFile()
        {
            string filepath = GetDynamicPath(_filename);
            byte[] jsonBytes = Encoding.UTF8.GetBytes("{}");
            using (FileStream fs = File.Create(filepath))
            {
                fs.Write(jsonBytes, 0, jsonBytes.Length);
            }
        }

        /// <summary>
        /// Checks and repairs the integrity of the JSON file at the specified path.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool CheckFileIntegrity()
        {
            string filepath = GetDynamicPath(_filename);
            if (!File.Exists(filepath))
            {
                CreateJsonFile();
                return false;
            }

            string content = File.ReadAllText(filepath);
            bool validJson = ManageSavedData.IsValidJson(content);
            if (!validJson)
            {
                HandleBadFile();
            }

            return validJson;
        }

        private static void HandleBadFile()
        {
            string filepath = GetDynamicPath(_filename);
            string errorMsg = $"The file: {filepath} is broken and will be replaced. " +
                $"You can take the data from the file manually. Filepath copied to Clipboard!";
            
            MessageBox.Show(
                errorMsg,
                "ERROR", 
                MessageBoxButton.OK);

            string brokenFilePath = GetDynamicPath("BrokenFile.json");
            if (File.Exists(brokenFilePath))
            {
                File.Delete(brokenFilePath);
            }

            File.Move(filepath, brokenFilePath);
            Clipboard.SetText(brokenFilePath);
            CreateJsonFile();
        }

        public static void ResetFile()
            => File.WriteAllText(GetDynamicPath(_filename), "{}");

        public static void WriteAllText(string content)
        {
            string filepath = GetDynamicPath(_filename);
            File.WriteAllText(filepath, content);
        }

        public static string ReadAllText()
        {
            string filepath = GetDynamicPath(_filename);
            return File.ReadAllText(filepath);
        }
    }
}
