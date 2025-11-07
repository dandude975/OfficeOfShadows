using System;
using System.IO;
using System.Text.Json;

namespace OOS.Shared
{
    public class ProgressState
    {
        public string Checkpoint { get; set; } = "intro";
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
        public System.Collections.Generic.HashSet<string> Flags { get; set; } = new();
        public string Version { get; set; } = "Pre-Release 1.1.0";
    }

    public static class Progress
    {
        private static string PathFor(string baseDir) =>
            System.IO.Path.Combine(baseDir, "Saves", "save.json");


        public static bool Exists(string baseDir) => File.Exists(PathFor(baseDir));

        public static ProgressState Load(string baseDir)
        {
            var p = PathFor(baseDir);
            if (!File.Exists(p)) return new ProgressState();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<ProgressState>(json) ?? new ProgressState();
        }

        public static void Save(string baseDir, ProgressState s)
        {
            s.UpdatedUtc = DateTime.UtcNow;
            var p = PathFor(baseDir);
            Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            File.WriteAllText(p, JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static bool IsAtOrBeyond(string baseDir, string checkpoint) =>
            string.Compare(Load(baseDir).Checkpoint, checkpoint, StringComparison.Ordinal) >= 0;
    }
}