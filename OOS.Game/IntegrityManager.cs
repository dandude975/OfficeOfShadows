using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using OOS.Shared;

namespace OOS.Game
{
    public static class IntegrityManager
    {
        public static string ValidateAndRepair(string manifestPath, string sandboxRoot)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string reportPath = Path.Combine(sandboxRoot, $"integrity_{timestamp}.txt");

                using var writer = new StreamWriter(reportPath, false);
                writer.WriteLine("Office of Shadows – Integrity Check");
                writer.WriteLine($"Manifest : {manifestPath}");
                writer.WriteLine($"Sandbox  : {sandboxRoot}");
                writer.WriteLine($"Date     : {DateTime.Now}\n");

                if (!File.Exists(manifestPath))
                {
                    writer.WriteLine("FATAL: Manifest file not found!");
                    SharedLogger.Warn($"Manifest file missing: {manifestPath}");
                    return reportPath;
                }

                SandboxManifest manifest;
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    manifest = JsonSerializer.Deserialize<SandboxManifest>(json);
                }
                catch (Exception ex)
                {
                    writer.WriteLine($"FATAL: The JSON value could not be converted.\nError: {ex.Message}");
                    SharedLogger.Warn($"Invalid manifest JSON: {ex.Message}");
                    return reportPath;
                }

                var discrepancies = manifest.Validate(sandboxRoot);

                if (discrepancies.Count == 0)
                {
                    writer.WriteLine("All files verified successfully. No issues found.");
                }
                else
                {
                    writer.WriteLine($"Found {discrepancies.Count} issue(s). Attempting auto-repair where possible…\n");

                    foreach (var item in discrepancies)
                    {
                        writer.WriteLine($"- Missing: {item.Name}");
                        bool repaired = TryRepair(item, sandboxRoot, writer);

                        writer.WriteLine(repaired
                            ? "  Successfully repaired.\n"
                            : "  Could not repair: Target EXE not found.\n");
                    }
                }

                writer.Flush();
                SharedLogger.Info($"Integrity check completed. Report: {reportPath}");
                return reportPath;
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Integrity check failed: {ex.Message}");
                return null;
            }
        }

        private static bool TryRepair(ManifestItem item, string sandboxRoot, StreamWriter writer)
        {
            try
            {
                string targetPath = Path.Combine(sandboxRoot, item.Name);

                switch (item.Kind)
                {
                    case ItemKind.Directory:
                        Directory.CreateDirectory(targetPath);
                        writer.WriteLine("  Repaired: Created directory.");
                        return true;

                    case ItemKind.Shortcut:
                        ShortcutHelper.CreateShortcutForApp(
                            sandboxRoot,
                            item.Name,
                            item.TargetExe,
                            item.Description ?? "In-game shortcut");
                        writer.WriteLine("  Repaired: Recreated shortcut.");
                        return true;

                    case ItemKind.File:
                        File.WriteAllText(targetPath, item.ExpectedContent ?? "");
                        writer.WriteLine("  Repaired: Recreated placeholder file.");
                        return true;

                    default:
                        writer.WriteLine("  Skipped: Unsupported item type.");
                        return false;
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"  Repair failed: {ex.Message}");
                return false;
            }
        }
    }

    // Minimal manifest definitions
    public enum ItemKind { File, Directory, Shortcut }

    public class ManifestItem
    {
        public string Name { get; set; }
        public ItemKind Kind { get; set; }
        public string TargetExe { get; set; }
        public string Description { get; set; }
        public string ExpectedContent { get; set; }
    }

    public class SandboxManifest
    {
        public List<ManifestItem> Items { get; set; } = new();

        public List<ManifestItem> Validate(string sandboxRoot)
        {
            var missing = new List<ManifestItem>();
            foreach (var item in Items)
            {
                string path = Path.Combine(sandboxRoot, item.Name);

                bool exists = item.Kind switch
                {
                    ItemKind.Directory => Directory.Exists(path),
                    _ => File.Exists(path)
                };

                if (!exists)
                    missing.Add(item);
            }
            return missing;
        }
    }
}
