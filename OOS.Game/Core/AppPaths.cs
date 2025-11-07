using System;
using System.IO;
using System.Linq;

namespace OOS.Game
{
    internal static class AppPaths
    {
        // Base folders
        public static string BaseDir { get; } = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        public static string AssetsDir => Path.Combine(BaseDir, "Assets");
        public static string SavesDir => Path.Combine(BaseDir, "Saves");
        public static string ReportsDir => Path.Combine(BaseDir, "FileValidation");
        public static string VideosDir => AssetsDir; // keep videos in Assets for now

        // Sandbox - the player workspace on Desktop
        public static string SandboxRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Office Work Stuff");

        // Common executables we link to
        public static string TerminalExe => Path.Combine(BaseDir, "..", "..", "..", "OOS.Terminal", "bin", "Debug", "net8.0-windows", "OOS.Terminal.exe");
        public static string DeviceManagerExe => Path.Combine(BaseDir, "..", "..", "..", "OOS.DeviceManager", "bin", "Debug", "net8.0-windows", "OOS.DeviceManager.exe");

        // Helpers
        public static void EnsureAllDirectories()
        {
            Directory.CreateDirectory(SavesDir);
            Directory.CreateDirectory(ReportsDir);
            Directory.CreateDirectory(SandboxRoot);
        }

        public static string VideoPath(string fileName) => Path.Combine(VideosDir, fileName);
        public static string SavePath(string fileName) => Path.Combine(SavesDir, fileName);
        public static string ReportPath(string fileName) => Path.Combine(ReportsDir, fileName);
        public static string SandboxPath(params string[] more) => Path.Combine(new[] { SandboxRoot }.Concat(more).ToArray());
    }
}
