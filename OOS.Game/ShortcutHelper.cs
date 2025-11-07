using System;
using System.IO;

namespace OOS.Game
{
    internal static class ShortcutHelper
    {
        /// <summary>
        /// Creates a .lnk shortcut pointing to an EXE. Returns true on success.
        /// </summary>
        public static bool CreateShortcutForApp(string exePath, string shortcutPath, string? arguments = null, string? iconPath = null)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);

                // Use late-binding to WScript.Shell so we don't need to add IWshRuntimeLibrary reference.
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return false;

                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                if (!string.IsNullOrWhiteSpace(arguments)) shortcut.Arguments = arguments;
                if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath)) shortcut.IconLocation = iconPath;

                shortcut.Save();
                return File.Exists(shortcutPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
