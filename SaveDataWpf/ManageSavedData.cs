using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace SaveDataWpf
{
    internal sealed class ManageSavedData
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static void SaveData(KeyValuePair<string, SavedContent> pair)
        {
            if (!FileSystemHelper.CheckFileIntegrity())
                return;
                
            JsonObject jsonObject = JsonNode.Parse(FileSystemHelper.ReadAllText())!.AsObject();
            jsonObject[pair.Key] = JsonSerializer.SerializeToNode(pair.Value)!;

            FileSystemHelper.WriteAllText(jsonObject.ToString());
        }

        public static Dictionary<string, SavedContent> LoadData()
        {
            if (!FileSystemHelper.CheckFileIntegrity())
            {
                return [];
            }

            string content = FileSystemHelper.ReadAllText();
            return JsonSerializer.Deserialize<Dictionary<string, SavedContent>>(content, _jsonSerializerOptions)!;
        }

        public static void DeleteData(string key)
        {
            if (FileSystemHelper.CheckFileIntegrity())
            {
                string content = FileSystemHelper.ReadAllText();
                JsonObject jsonObj = JsonNode.Parse(content)!
                .AsObject();
                jsonObj.Remove(key);

                FileSystemHelper.WriteAllText(jsonObj.ToJsonString(_jsonSerializerOptions));
                MainWindow? window = Application.Current.MainWindow as MainWindow;
                Dictionary<string, SavedContent> dict = JsonSerializer.Deserialize<Dictionary<string, SavedContent>>(jsonObj.ToJsonString(_jsonSerializerOptions), _jsonSerializerOptions)!;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window?.ReloadListBox(dict);
                });
            }
        }
        
        internal static void UpdateData(KeyValuePair<string, SavedContent>? oldPair, KeyValuePair<string, SavedContent> newPair)
        {
            if (!FileSystemHelper.CheckFileIntegrity())
            {
                return;
            }

            JsonObject jsonObj = JsonNode.Parse(FileSystemHelper.ReadAllText())!.AsObject();
            if (oldPair.HasValue && oldPair.Value.Key != newPair.Key)
                jsonObj.Remove(oldPair.Value.Key);

            jsonObj[newPair.Key] = new JsonObject
            {
                ["content"] = newPair.Value.Content,
                ["isEncrypted"] = newPair.Value.IsEncrypted
            };

            string jsonString = jsonObj.ToJsonString(_jsonSerializerOptions);
            FileSystemHelper.WriteAllText(jsonString);

            MainWindow? window = Application.Current.MainWindow as MainWindow;
            window?.ReloadListBox(JsonSerializer.Deserialize<Dictionary<string, SavedContent>>(jsonString, _jsonSerializerOptions)!);
        }

        public static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
