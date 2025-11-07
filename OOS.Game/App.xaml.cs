using System;
using System.IO;
using System.Windows;
using OOS.Shared;

namespace OOS.Game
{
    public partial class App : Application
    {
        private const string IntroVideo = "excoworker_clip.mp4";
        private const string ResumeVideo = "resume.mp4"; // optional fallback

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppPaths.EnsureAllDirectories();

            // Decide: first boot (no save) => disclaimer + intro video
            // otherwise show resume window
            var saveFile = AppPaths.SavePath("save.json");
            bool hasSave = File.Exists(saveFile);

            if (!hasSave)
            {
                // Disclaimer/intro window (your existing IntroWindow.xaml)
                var intro = new IntroWindow();
                var ok = intro.ShowDialog() == true;

                if (!ok) { Shutdown(); return; }

                // Play intro video slightly larger than intro window
                ShowVideoAndWait(IntroVideo);

                // Create initial save at "post_intro"
                var save = new SaveData { Checkpoint = "post_intro", Flags = [], TimestampUtc = DateTime.UtcNow };
                GameSave.Save(saveFile, save);
            }
            else
            {
                var resume = new ResumeWindow();
                var res = resume.ShowDialog();

                if (res == true && resume.NewGameChosen)
                {
                    // wipe save and play intro again
                    try { File.Delete(saveFile); } catch { }
                    ShowVideoAndWait(IntroVideo);
                    var fresh = new SaveData { Checkpoint = "post_intro", Flags = [], TimestampUtc = DateTime.UtcNow };
                    GameSave.Save(saveFile, fresh);
                }
                else if (res == true && resume.ContinueChosen)
                {
                    // optional tiny video or skip straight on
                    if (File.Exists(AppPaths.VideoPath(ResumeVideo)))
                        ShowVideoAndWait(ResumeVideo);
                }
                else
                {
                    Shutdown(); return;
                }
            }

            // Deploy workspace every run (idempotent)
            try
            {
                ToolDeployer.DeployWorkspace();
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Workspace deploy failed: {ex.Message}");
            }

            // Integrity check => write report to FileValidation, do not open UI
            try
            {
                IntegrityManager.ValidateAndRepair(
                    Path.Combine(AppPaths.AssetsDir, "manifest.json"),
                    AppPaths.SandboxRoot,
                    AppPaths.ReportsDir);
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Integrity check skipped: {ex.Message}");
            }

            // Open sandbox folder
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = AppPaths.SandboxRoot,
                    UseShellExecute = true
                });
            }
            catch { }

            // Start background manager (keeps app alive + popups later)
            BackgroundManager.Instance.Start();

            // Hide “main window” (we’re background now)
            var ghost = new Window { Width = 0, Height = 0, Visibility = Visibility.Hidden, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
            MainWindow = ghost;
            MainWindow.Show();
        }

        private static void ShowVideoAndWait(string fileName)
        {
            var path = AppPaths.VideoPath(fileName);
            var vw = new VideoWindow(path);
            vw.ShowDialog();
        }
    }
}
