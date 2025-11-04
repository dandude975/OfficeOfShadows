using System;
using System.IO;
using System.Windows;

namespace OOS.Game
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var intro = new IntroWindow();
            intro.ShowDialog();

            if (!intro.UserConsented)
            {
                Shutdown();
                return;
            }

            // Compute a safe position: prefer Intro’s saved position; otherwise center on work area
            (double left, double top) PosOrCenter(double w, double h)
            {
                bool finiteL = !double.IsNaN(intro.SavedLeft) && !double.IsInfinity(intro.SavedLeft);
                bool finiteT = !double.IsNaN(intro.SavedTop) && !double.IsInfinity(intro.SavedTop);

                if (finiteL && finiteT)
                    return (intro.SavedLeft, intro.SavedTop);

                var wa = SystemParameters.WorkArea;
                var cx = wa.Left + (wa.Width - w) / 2.0;
                var cy = wa.Top + (wa.Height - h) / 2.0;
                return (cx, cy);
            }

            var clipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "excoworker_clip.mp4");
            if (File.Exists(clipPath))
            {
                // Use the intro-sized window as a baseline (tweak these to taste)
                var (left, top) = PosOrCenter(900, 560);
                var video = new VideoWindow(clipPath, left, top);
                await video.PlayAsync();
            }

            var sandboxPath = SandboxHelper.EnsureSandboxFolder();
            ShortcutHelper.CreateShortcutsIfMissing(sandboxPath);
            System.Diagnostics.Process.Start("explorer.exe", sandboxPath);

            BackgroundManager.Instance.Start();
            TrayIconManager.CreateTrayIcon();
        }
    }
}
