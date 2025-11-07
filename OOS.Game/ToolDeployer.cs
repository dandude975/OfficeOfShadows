using System.IO;
using System.Text;

namespace OOS.Game
{
    internal static class ToolDeployer
    {
        public static void DeployWorkspace()
        {
            // Ensure base folder + Notes
            Directory.CreateDirectory(AppPaths.SandboxRoot);
            Directory.CreateDirectory(AppPaths.SandboxPath("Notes"));

            // README (idempotent)
            var readmePath = AppPaths.SandboxPath("README.txt");
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath,
@"Welcome to the Office Work Stuff folder.

This is your workspace for the investigation. Tools appear here as shortcuts.
Notes you take can go into the Notes folder. Most items will be recreated if
you accidentally delete them. Have fun.");
            }

            // Shortcuts
            var termLnk = AppPaths.SandboxPath("Terminal.lnk");
            var devLnk = AppPaths.SandboxPath("DeviceManager.lnk");

            if (File.Exists(AppPaths.TerminalExe))
                ShortcutHelper.TryCreateShortcutForApp(AppPaths.TerminalExe, termLnk);

            if (File.Exists(AppPaths.DeviceManagerExe))
                ShortcutHelper.TryCreateShortcutForApp(AppPaths.DeviceManagerExe, devLnk);
        }
    }
}
