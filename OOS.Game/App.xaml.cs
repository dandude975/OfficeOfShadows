using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using OOS.Shared; // your logger namespace if you use it

namespace OOS.Game
{
    public partial class App : Application
    {
        public static string BaseDir => AppContext.BaseDirectory;
        public static string AssetsDir => Path.Combine(BaseDir, "Assets");
        public static string VideosDir => Path.Combine(AssetsDir, "Videos");
        public static string SandboxRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Office Work Stuff");

        protected override void OnStartup(StartupEventArgs e)
        {
            // Prevent app from exiting when the first window closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            base.OnStartup(e);

            try
            {
                Directory.CreateDirectory(SandboxRoot);
                Directory.CreateDirectory(Path.Combine(SandboxRoot, "IntegrityReports"));

                // Always ensure shortcuts are present
                ShortcutHelper.ValidateShortcuts(SandboxRoot);

                // Decide intro vs resume
                var savePath = Path.Combine(SandboxRoot, "save.json");
                bool hasSave = File.Exists(savePath);

                if (!hasSave)
                {
                    // First run → disclaimer / intro window
                    var intro = new IntroWindow(); // your existing disclaimer window
                    bool? ok = intro.ShowDialog();
                    if (ok != true)
                    {
                        // User closed/cancelled → just exit cleanly
                        Shutdown();
                        return;
                    }

                    // Mark first checkpoint so we won’t replay intro next time
                    var save = new GameSave
                    {
                        Checkpoint = "post_intro",
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    File.WriteAllText(savePath, JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true }));

                    // Play intro video
                    PlayVideoBlocking("intro.mp4");
                }
                else
                {
                    // Resume flow
                    var resume = new ResumeWindow();  // shows timestamp + continue/new
                    resume.ShowDialog();

                    // (Optionally) a shorter resume clip
                    PlayVideoBlocking("resume.mp4");
                }

                // After video: integrity & open sandbox
                RunIntegrityAndReveal();

                // All done
                Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup failed:\n{ex.Message}", "Office of Shadows",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private static void PlayVideoBlocking(string fileName)
        {
            try
            {
                string path = Path.Combine(VideosDir, fileName);
                if (!File.Exists(path))
                {
                    MessageBox.Show($"Video file not found:\n{path}",
                                    "Video Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var win = new VideoWindow(fileName);  // must ShowDialog to block
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Video playback error:\n{ex.Message}",
                                "Video Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void RunIntegrityAndReveal()
        {
            try
            {
                string manifest = Path.Combine(AssetsDir, "manifest.json");
                string report = IntegrityManager.ValidateAndRepair(manifest, SandboxRoot);

                if (!string.IsNullOrEmpty(report) && File.Exists(report))
                {
                    // move into Office Work Stuff\IntegrityReports
                    string reportsDir = Path.Combine(SandboxRoot, "IntegrityReports");
                    Directory.CreateDirectory(reportsDir);
                    string final = Path.Combine(reportsDir, Path.GetFileName(report));
                    File.Move(report, final, true);

                    // Optional: small popup confirmation (so you know it ran)
                    MessageBox.Show("File integrity check complete. Opening report and workspace folder.",
                                    "Integrity", MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start("notepad.exe", final);
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SandboxRoot,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Integrity step failed:\n{ex.Message}",
                                "Integrity Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class GameSave
    {
        public string Checkpoint { get; set; }
        public string Timestamp { get; set; }
    }
}
