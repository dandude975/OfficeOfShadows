using System;
using System.Collections.Generic;
using System.IO;
using OOS.Shared;

namespace OOS.Game
{
    internal static class IntegrityManager
    {
        public sealed class Result
        {
            public string ReportPath { get; init; } = "";
            public int IssueCount { get; init; }
            public bool ManifestFound { get; init; }
        }

        /// <summary>
        /// Validates the sandbox against the manifest; attempts simple repairs; writes a report.
        /// Returns where the report was written and how many issues were found.
        /// </summary>
        public static Result RunStartupCheck(string manifestPath, string sandboxRoot)
        {
            var reportLines = new List<string>();
            reportLines.Add($"Office of Shadows – Integrity Check");
            reportLines.Add($"Manifest: {manifestPath}");
            reportLines.Add($"Sandbox : {sandboxRoot}");
            reportLines.Add($"Date    : {DateTime.Now}");
            reportLines.Add("");

            try
            {
                if (!File.Exists(manifestPath))
                {
                    reportLines.Add("ERROR: Manifest not found. Skipping validation.");
                    var path = WriteReport(sandboxRoot, reportLines);
                    SharedLogger.Info($"Integrity report written (no manifest): {path}");
                    return new Result { ReportPath = path, IssueCount = -1, ManifestFound = false };
                }

                var manifest = SandboxManifest.Load(manifestPath);
                var issues = new List<Discrepancy>(manifest.Validate(sandboxRoot));

                if (issues.Count == 0)
                {
                    reportLines.Add("All items match the manifest.");
                    var path = WriteReport(sandboxRoot, reportLines);
                    SharedLogger.Info($"Integrity report written (no issues): {path}");
                    return new Result { ReportPath = path, IssueCount = 0, ManifestFound = true };
                }

                reportLines.Add($"Found {issues.Count} issue(s). Attempting auto-repair where possible…");
                reportLines.Add("");

                foreach (var d in issues)
                {
                    reportLines.Add($"- {d.Kind}: {d.Item.Path} ({d.Details})");
                    try
                    {
                        switch (d.Item.Kind)
                        {
                            case ItemKind.Directory:
                                if (!Directory.Exists(d.FullPath))
                                {
                                    Directory.CreateDirectory(d.FullPath);
                                    reportLines.Add("  Repaired: Created directory.");
                                }
                                break;

                            case ItemKind.Shortcut:
                                {
                                    var ok = ShortcutHelper.CreateShortcutForApp(
                                        sandboxRoot,
                                        d.Item.Path,               // e.g., "Terminal.lnk"
                                        AppNameFromShortcut(d.Item.Path), // map link name -> project
                                        "Office of Shadows tool"
                                    );
                                    reportLines.Add(ok
                                        ? "  Repaired: Recreated shortcut."
                                        : "  Could not repair: Target EXE not found.");
                                    break;
                                }

                            case ItemKind.File:
                                if (!string.IsNullOrWhiteSpace(d.Item.Source))
                                {
                                    var src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, d.Item.Source);
                                    if (File.Exists(src))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(d.FullPath)!);
                                        File.Copy(src, d.FullPath, overwrite: true);
                                        reportLines.Add($"  Repaired: Copied from {d.Item.Source}");
                                    }
                                    else
                                    {
                                        reportLines.Add($"  Could not repair: Source not found: {src}");
                                    }
                                }
                                else if (string.Equals(Path.GetFileName(d.FullPath), "README.txt", StringComparison.OrdinalIgnoreCase))
                                {
                                    File.WriteAllText(d.FullPath, DefaultReadme());
                                    reportLines.Add("  Repaired: Regenerated README content.");
                                }
                                else
                                {
                                    reportLines.Add("  No repair source specified.");
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        reportLines.Add($"  Repair failed: {ex.Message}");
                    }
                }

                var written = WriteReport(sandboxRoot, reportLines);
                SharedLogger.Info($"Integrity report written (issues {issues.Count}): {written}");
                return new Result { ReportPath = written, IssueCount = issues.Count, ManifestFound = true };
            }
            catch (Exception ex)
            {
                reportLines.Add("");
                reportLines.Add($"FATAL: {ex.Message}");
                var written = WriteReport(sandboxRoot, reportLines);
                SharedLogger.Error($"Integrity check crashed; report at: {written}");
                return new Result { ReportPath = written, IssueCount = -2, ManifestFound = false };
            }
        }

        private static string AppNameFromShortcut(string linkPath)
        {
            var name = Path.GetFileNameWithoutExtension(linkPath).ToLowerInvariant();
            return name switch
            {
                "terminal" => "OOS.Terminal",
                "vpn" => "OOS.VPN",
                "email" => "OOS.Email",
                _ => "OOS.Terminal" // default
            };
        }


        /// <summary>
        /// Prefer writing to EXE\FileValidation. If that fails, fall back to sandbox.
        /// Returns the path actually written.
        /// </summary>
        private static string WriteReport(string sandboxRoot, List<string> lines)
        {
            // primary location: alongside game exe
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var outDir = Path.Combine(baseDir, "FileValidation");
            var file = Path.Combine(outDir, $"integrity_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            try
            {
                Directory.CreateDirectory(outDir);
                File.WriteAllLines(file, lines);
                return file;
            }
            catch
            {
                // fallback: sandbox
                try
                {
                    var fallback = Path.Combine(sandboxRoot, "_integrity_report.txt");
                    File.WriteAllLines(fallback, lines);
                    return fallback;
                }
                catch
                {
                    return ""; // ultimate fallback: nothing we can do
                }
            }
        }

        private static string DefaultReadme() =>
@"OFFICE OF SHADOWS – Liam’s Info

This folder is where your investigation tools, notes, and clues will appear.

If items are missing, the game will attempt to repair them automatically on startup.
You can reopen the game to rebuild critical files and shortcuts.

– RETIS Software";
    }
}
