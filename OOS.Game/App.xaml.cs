using System;
using System.Threading.Tasks;
using System.Windows;

namespace OOS.Game
{
    public partial class App : Application
    {
        private BackgroundManager? _bg;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Prevent app from auto-shutting down when last Window closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Show disclaimers / consent
            var intro = new IntroWindow();
            intro.ShowDialog(); // modal - blocks until user accepts/starts

            if (!intro.UserConsented)
            {
                Shutdown();
                return;
            }

            // Play the “ex-coworker” video (modal). Wait for it to finish.
            var videoWin = new VideoWindow("Assets/excoworker_clip.mp4");
            await videoWin.PlayAsync();

            // Setup sandbox and shortcuts then open the folder in Explorer
            var sandboxPath = SandboxHelper.EnsureSandboxFolder();
            ShortcutHelper.CreateShortcutsIfMissing(sandboxPath /*, list of exe names */);
            System.Diagnostics.Process.Start("explorer.exe", sandboxPath);

            // Start background manager (keeps running even if no windows open)
            _bg = BackgroundManager.Instance;
            _bg.Start();

            // Hide/close the intro/video windows (they are already closed) 
            // Optionally show tray icon so player can access Reset/Quit
            TrayIconManager.CreateTrayIcon();

            // Keep application alive (no visible main window)
            // Application will be stopped explicitly via Reset/Quit in tray
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _bg?.Stop();
            TrayIconManager.DisposeTrayIcon();
            base.OnExit(e);
        }
    }
}
