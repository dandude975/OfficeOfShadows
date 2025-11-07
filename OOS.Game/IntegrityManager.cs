using System;
using System.IO;
using System.Text.Json;

namespace OOS.Game
{
    internal static class IntegrityManager
    {
        private record ManifestItem(string kind, string path); // very light schema

        public static void ValidateAndRepair(string manifestPath, string sandboxRoot, string reportDir)
        {
            Directory.CreateDirectory(reportDir);

            string reportFile = Path.Combine(reportDir, $"integrity_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt");
            using var writer = new StreamWriter(reportFile);

            writer.WriteLine("Office of Shadows – Integrity Check");
            writer.WriteLine($"Manifest : {manifestPath}");
            writer.WriteLine($"Sandbox  : {sandboxRoot}");
            writer.WriteLine($"Date     : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine();

            ManifestItem[] items = Array.Empty<ManifestItem>();

            try
            {
                if (File.Exists(manifestPath))
                {
                    var json = File.ReadAllText(manifestPath);
                    items = JsonSerializer.Deserialize<ManifestItem[]>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? Array.Empty<ManifestItem>();
                }
                else
                {
                    writer.WriteLine("NOTE: Manifest not found; proceeding with deploy-only mode.");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"FATAL: Could not parse manifest: {ex.Message}");
            }

            // Minimal repairs we always want:
            Directory.CreateDirectory(sandboxRoot);
            Directory.CreateDirectory(Path.Combine(sandboxRoot, "Notes"));

            // If manifest present, ensure listed paths exist
            foreach (var it in items)
            {
                try
                {
                    var full = Path.Combine(sandboxRoot, it.path);
                    if (it.kind.Equals("dir", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Directory.Exists(full))
                        {
                            Directory.CreateDirectory(full);
                            writer.WriteLine($"Repaired: Created directory {it.path}");
                        }
                    }
                    else if (it.kind.Equals("file", StringComparison.OrdinalIgnoreCase))
                    {
                        // we'll only mark; content population is handled by ToolDeployer/shortcuts
                        if (!File.Exists(full))
                            writer.WriteLine($"Missing file noted: {it.path}");
                    }
                    else if (it.kind.Equals("shortcut", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!File.Exists(full))
                            writer.WriteLine($"Missing shortcut noted: {it.path}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine($"ERROR on {it.path}: {ex.Message}");
                }
            }

            writer.WriteLine();
            writer.WriteLine("Integrity pass complete.");
        }
    }
}
