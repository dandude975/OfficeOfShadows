using System;
using System.Diagnostics;
using System.IO;

namespace OOS.Game
{
    /// <summary>Central place for all game paths.</summary>
    internal static class AppPaths
    {
        public static string ExecutablePath =>
            Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory;

        public static string BaseDir => Path.GetDirectoryName(ExecutablePath)!;

        public static string AssetsDir => Path.Combine(BaseDir, "Assets");
        public static string VideosDir => Path.Combine(AssetsDir, "Videos");
        public static string ManifestPath => Path.Combine(AssetsDir, "manifest.json");

        // Player-facing sandbox on Desktop
        public static string SandboxRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Office Work Stuff");

        // Game-internal save + integrity locations (inside the game bin dir)
        public static string SaveDir => Path.Combine(BaseDir, "Saves");
        public static string IntegrityDir => Path.Combine(BaseDir, "FileValidation");

        public static string GetAssetPath(string relative) => Path.Combine(AssetsDir, relative);

        public static void EnsureAllDirectories()
        {
            Directory.CreateDirectory(SandboxRoot);
            Directory.CreateDirectory(SaveDir);
            Directory.CreateDirectory(IntegrityDir);

            // the one sandbox subfolder we want to create up-front
            Directory.CreateDirectory(Path.Combine(SandboxRoot, "Notes"));
        }
    }
}
