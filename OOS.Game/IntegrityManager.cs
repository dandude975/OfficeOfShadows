using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using OOS.Shared;

namespace OOS.Game
{
    internal static class IntegrityManager
    {
        // manifest model
        private sealed class ManifestFile
        {
            public ManifestItem[] Items { get; set; } = Array.Empty<ManifestItem>();
        }

        private sealed class ManifestItem
        {
            // "dir" | "file" | "lnk"
            public string Kind { get; set; } = "";
            // relative path inside sandbox, e.g. "Notes" or "Terminal.lnk"
            public string Path { get; set; } = "";
            // for lnk/file: optional source or target exe name, e.g. "OOS.Terminal.exe"
            public string? Target { get; set; }
            // for file copy from assets: optional asset relative path
            public string? Asset { get; set; }
        }

        /// <summary>
        /// Validates the sandbox content against the manifest and auto-repairs where possible.
        /// Returns the full path to the report file (written in reportDir).
        /// </summary>
        public static string ValidateAndRepair(string manifestPath, string sandboxDir, string reportDir)
        {
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, $"integrity_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            var sb = new StringBuilder();

            sb.AppendLine("Office of Shadows – Integrity Check");
            sb.AppendLine($"Manifest : {manifestPath}");
            sb.AppendLine($"Sandbox  : {sandboxDir}");
            sb.AppendLine($"Date     : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            ManifestFile? manifest = null;
            try
            {
                if (!File.Exists(manifestPath))
                {
                    sb.AppendLine("FATAL: manifest not found.");
                    File.WriteAllText(reportPath, sb.ToString());
                    return reportPath;
                }

                var json = File.ReadAllText(manifestPath);
                manifest = JsonSerializer.Deserialize<ManifestFile>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest?.Items == null)
                {
                    sb.AppendLine("FATAL: could not deserialize manifest.");
                    File.WriteAllText(reportPath, sb.ToString());
                    return reportPath;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"FATAL: Exception reading manifest: {ex.Message}");
                File.WriteAllText(reportPath, sb.ToString());
                return reportPath;
            }

            var issues = 0;

            foreach (var item in manifest.Items)
            {
                try
                {
                    var targetPath = Path.Combine(sandboxDir, item.Path);

                    switch ((item.Kind ?? "").ToLowerInvariant())
                    {
                        case "dir":
                            if (!Directory.Exists(targetPath))
                            {
                                Directory.CreateDirectory(targetPath);
                                sb.AppendLine($"- Missing: {item.Path} (dir)");
                                sb.AppendLine("  Repaired: Created directory.");
                                issues++;
                            }
                            break;

                        case "file":
                            {
                                if (!File.Exists(targetPath))
                                {
                                    sb.AppendLine($"- Missing: {item.Path} (file)");
                                    // copy from assets if provided
                                    if (!string.IsNullOrWhiteSpace(item.Asset))
                                    {
                                        var src = AppPaths.GetAssetPath(item.Asset);
                                        if (File.Exists(src))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                                            File.Copy(src, targetPath, overwrite: true);
                                            sb.AppendLine("  Repaired: Copied from assets.");
                                            issues++;
                                        }
                                        else
                                        {
                                            sb.AppendLine("  Could not repair: asset missing.");
                                        }
                                    }
                                    else
                                    {
                                        sb.AppendLine("  Could not repair: no asset specified.");
                                    }
                                }
                            }
                            break;

                        case "lnk":
                            {
                                if (!File.Exists(targetPath))
                                {
                                    sb.AppendLine($"- Missing: {item.Path} (shortcut)");
                                    var exe = ResolveToolExe(item.Target);
                                    if (exe != null)
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                                        ShortcutHelper.CreateShortcutForApp(exe, targetPath);
                                        sb.AppendLine("  Repaired: Recreated shortcut.");
                                        issues++;
                                    }
                                    else
                                    {
                                        sb.AppendLine("  Could not repair: target EXE not found.");
                                    }
                                }
                            }
                            break;

                        default:
                            sb.AppendLine($"WARN: Unknown item kind '{item.Kind}' for '{item.Path}'.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"ERROR processing '{item.Path}': {ex.Message}");
                }
            }

            if (issues == 0)
            {
                sb.AppendLine("No issues found.");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"Found {issues} issue(s).");
            }

            File.WriteAllText(reportPath, sb.ToString());
            return reportPath;
        }

        private static string? ResolveToolExe(string? targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName)) return null;

            // packaged: next to game exe
            var nearGame = Path.Combine(AppPaths.BaseDir, targetName);

            // dev fallback: sibling project's debug bin
            var sibling = Path.GetFullPath(Path.Combine(
                AppPaths.BaseDir, "..", "..", "..",
                Path.GetFileNameWithoutExtension(targetName)!, "bin", "Debug", "net8.0-windows", targetName));

            if (File.Exists(nearGame)) return nearGame;
            if (File.Exists(sibling)) return sibling;
            return null;
        }
    }
}
