using System;
using System.IO;

namespace OOS.Game
{
    internal static class ShortcutHelper
    {
        // Creates a simple .lnk using WSH COM if present; if not, we silently skip.
        public static void TryCreateShortcutForApp(string exePath, string linkPath, string arguments = "", string? iconPath = null)
        {
            try
            {
                var wshType = Type.GetTypeFromProgID("WScript.Shell");
                if (wshType == null) return;

                dynamic shell = Activator.CreateInstance(wshType)!;
                dynamic shortcut = shell.CreateShortcut(linkPath);

                shortcut.TargetPath = exePath;
                shortcut.Arguments = arguments ?? "";
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.WindowStyle = 1;
                if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
                    shortcut.IconLocation = iconPath;
                else
                    shortcut.IconLocation = exePath;
                shortcut.Save();
            }
            catch
            {
                // Non-fatal. We'll rely on auto-repair next run.
            }
        }
    }
}
