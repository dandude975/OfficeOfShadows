using System;
using System.IO;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace OOS.Game
{
    internal static class ShortcutHelper
    {
        public static void CreateShortcutsIfMissing(string sandboxDir)
        {
            Directory.CreateDirectory(sandboxDir);

            CreateShortcutForApp(sandboxDir, "Terminal.lnk", "OOS.Terminal.exe", "Launch the in-game terminal environment");
            CreateShortcutForApp(sandboxDir, "DeviceManager.lnk", "OOS.DeviceManager.exe", "Manage connected (real + simulated) devices");
        }

        public static void CreateShortcutForApp(string sandboxDir, string shortcutName, string exeName, string description)
        {
            try
            {
                string exePath = Path.Combine(App.BaseDir, exeName);
                string shortcutPath = Path.Combine(sandboxDir, shortcutName);

                if (!File.Exists(exePath))
                {
                    OOS.Shared.SharedLogger.Warn($"Target EXE not found for shortcut: {exePath}");
                    return;
                }

                if (File.Exists(shortcutPath)) return;

                var shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Description = description;
                shortcut.IconLocation = exePath;
                shortcut.Save();

                OOS.Shared.SharedLogger.Info($"Created shortcut: {shortcutPath}");
            }
            catch (Exception ex)
            {
                OOS.Shared.SharedLogger.Warn($"Failed to create shortcut {shortcutName}: {ex.Message}");
            }
        }

        public static void ValidateShortcuts(string sandboxDir)
        {
            var shortcuts = new[]
            {
                ("Terminal.lnk", "OOS.Terminal.exe", "Launch the in-game terminal environment"),
                ("DeviceManager.lnk", "OOS.DeviceManager.exe", "Manage connected (real + simulated) devices")
            };

            foreach (var (name, exe, desc) in shortcuts)
            {
                string shortcutPath = Path.Combine(sandboxDir, name);
                string exePath = Path.Combine(App.BaseDir, exe);

                bool needsRecreate = !File.Exists(shortcutPath)
                                     || !File.Exists(exePath)
                                     || !IsShortcutTargetValid(shortcutPath, exePath);

                if (needsRecreate)
                    CreateShortcutForApp(sandboxDir, name, exe, desc);
            }
        }

        private static bool IsShortcutTargetValid(string shortcutPath, string expectedTarget)
        {
            try
            {
                var shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                string actualTarget = shortcut.TargetPath ?? string.Empty;

                return string.Equals(
                    Path.GetFullPath(actualTarget),
                    Path.GetFullPath(expectedTarget),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
