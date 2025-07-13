using System.IO;
using System.Text;
using System.Windows;

namespace SaveDataWpf
{
    internal sealed class FileSystemHelper
    {
        private const string _filename = "SavedData.json";

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
            MessageBox.Show($"The file: {filepath} is broken and will be replaced. " +
                $"You can take the data from the file manually.", "ERROR", MessageBoxButton.OK);

            string brokenFilePath = GetDynamicPath("BrokenFile.json");
            File.Move(filepath, brokenFilePath);

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
