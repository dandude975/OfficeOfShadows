using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OOS.Shared
{
    /// <summary>
    /// Shape of your save file. Extend as needed (flags, progress, etc.).
    /// </summary>
    public sealed class SaveData
    {
        public string Checkpoint { get; set; } = "";
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        // Optional: arbitrary flags you want to persist
        public Dictionary<string, bool>? Flags { get; set; }
    }

    /// <summary>
    /// Simple JSON save system used across all apps.
    /// </summary>
    public static class GameSave
    {
        private const string SaveFileName = "save.json";

        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static string GetSavePath(string saveDir)
            => Path.Combine(saveDir, SaveFileName);

        /// <summary>
        /// Does a save file exist in the given directory?
        /// </summary>
        public static bool Exists(string saveDir)
            => File.Exists(GetSavePath(saveDir));

        /// <summary>
        /// Load the save from disk. Throws if the file doesn't exist.
        /// </summary>
        public static SaveData Load(string saveDir)
        {
            var path = GetSavePath(saveDir);
            if (!File.Exists(path))
                throw new FileNotFoundException("Save not found.", path);

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SaveData>(json, _json);
            return data ?? new SaveData();
        }

        /// <summary>
        /// Save to disk, creating the directory if needed.
        /// </summary>
        public static void Save(string saveDir, SaveData data)
        {
            Directory.CreateDirectory(saveDir);
            var path = GetSavePath(saveDir);
            var json = JsonSerializer.Serialize(data, _json);
            File.WriteAllText(path, json);
        }
    }
}
