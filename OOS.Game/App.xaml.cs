using System;
using System.Windows;
using OOS.Shared;

namespace OOS.Game
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Make sure all directories exist before proceeding
            AppPaths.EnsureAllDirectories();

            // If there’s an existing save file, go to ResumeWindow; otherwise play intro
            if (GameSave.Exists(AppPaths.SaveDir))
            {
                var resume = new ResumeWindow();
                resume.ShowDialog();
            }
            else
            {
                var intro = new IntroWindow();
                intro.ShowDialog();
            }

            // Start background services (tray icon etc.)
            BackgroundManager.Instance.Start();

            // Validate file integrity (writes report to FileValidation folder, doesn’t open)
            try
            {
                _ = IntegrityManager.ValidateAndRepair(
                    AppPaths.ManifestPath,
                    AppPaths.SandboxRoot,
                    AppPaths.IntegrityDir);
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Startup integrity check failed: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            BackgroundManager.Instance.Stop();
            base.OnExit(e);
        }
    }
}
