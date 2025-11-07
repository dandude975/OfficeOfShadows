using System;
using System.IO;

namespace OOS.Game
{
    internal static class ToolDeployer
    {
        // Try to find tool EXE next to game; if missing, copy from sibling project bin folders (dev)
        public static void EnsureToolsPresent()
        {
            TryEnsure("OOS.Terminal.exe", FindDevOutput("OOS.Terminal"));
            TryEnsure("OOS.DeviceManager.exe", FindDevOutput("OOS.DeviceManager"));
        }

        private static void TryEnsure(string exeName, string? devCandidate)
        {
            var dest = Path.Combine(App.BaseDir, exeName);
            if (File.Exists(dest)) return;

            if (!string.IsNullOrWhiteSpace(devCandidate) && File.Exists(devCandidate))
            {
                Directory.CreateDirectory(App.BaseDir);
                File.Copy(devCandidate, dest, overwrite: true);
                return;
            }

            OOS.Shared.SharedLogger.Warn($"Tool missing and not found in dev bins: {exeName}");
        }

        // Searches common dev output locations relative to the Game EXE (Debug/Release)
        private static string? FindDevOutput(string projectName)
        {
            try
            {
                // App.BaseDir = ...\OOS.Game\bin\Debug\net8.0-windows\
                var gameBin = App.BaseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var gameBinRoot = Directory.GetParent(gameBin)!.Parent!; // -> OOS.Game\bin

                foreach (var cfg in new[] { "Debug", "Release" })
                {
                    var dir = Path.Combine(
                        Directory.GetParent(gameBinRoot.FullName)!.FullName, // -> OOS.Game
                        projectName, "bin", cfg, "net8.0-windows",
                        $"{projectName}.exe");
                    if (File.Exists(dir)) return dir;
                }
            }
            catch { }
            return null;
        }
    }
}
