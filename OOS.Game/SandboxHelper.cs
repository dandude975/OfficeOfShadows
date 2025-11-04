using System;
using System.IO;

namespace OOS.Game
{
    public static class SandboxHelper
    {
        public static string SandboxFolderName => "Liam's Info";

        public static string EnsureSandboxFolder()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var path = Path.Combine(desktop, SandboxFolderName);
            Directory.CreateDirectory(path);

            var readme = Path.Combine(path, "README.txt");
            if (!File.Exists(readme))
                File.WriteAllText(readme, "Open the shortcuts to begin.\nAll files here are safe to delete.");

            return path;
        }
    }
}
