using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OOS.Shared
{
    public class SaveData
    {
        public string Checkpoint { get; set; } = "";
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public HashSet<string> Flags { get; set; } = new();
    }

    public static class GameSave
    {
        public static void Save(string path, SaveData data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static SaveData? TryLoad(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<SaveData>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        public static SaveData Load(string path) => TryLoad(path) ?? new SaveData();
    }
}
