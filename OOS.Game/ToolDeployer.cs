using System.IO;

namespace OOS.Game
{
    internal static class ToolDeployer
    {
        public static void EnsureNotesFolder()
        {
            var notesPath = Path.Combine(AppPaths.SandboxRoot, "Notes");
            Directory.CreateDirectory(notesPath);
        }
    }
}
