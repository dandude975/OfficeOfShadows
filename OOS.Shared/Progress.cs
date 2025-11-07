using System;
using System.IO;
using System.Text.Json;

namespace OOS.Shared
{
    public class Progress
    {
        public string Checkpoint { get; set; } = "intro";    // current checkpoint id
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, bool> Flags { get; set; } = new(); // arbitrary toggles

        public static Progress Load()
        {
            var path = SharedPaths.ProgressFile;
            if (!File.Exists(path)) return new Progress();
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Progress>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Progress();
            }
            catch { return new Progress(); }
        }

        public void Save()
        {
            UpdatedUtc = DateTime.UtcNow;
            Directory.CreateDirectory(Path.GetDirectoryName(SharedPaths.ProgressFile)!);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SharedPaths.ProgressFile, json);
        }

        public bool IsAtOrBeyond(string checkpoint) => CheckpointOrder.IndexOf(Checkpoint) >= CheckpointOrder.IndexOf(checkpoint);

        // Define your linear order here (you can make it graph-based later)
        public static readonly List<string> CheckpointOrder = new()
        {
            "intro",
            "video_played",
            "tools_opened",
            "first_email_read",
            "vpn_connected",
            "terminal_scan_done",
            "report_compiled",
            "game_complete"
        };
    }
}
