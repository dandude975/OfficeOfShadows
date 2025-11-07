using OOS.Shared;
using System;
using System.IO;
using System.Windows;

namespace OOS.Game
{
    public partial class App : Application
    {
        private StoryController _story = new();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (!_story.AtLeast("video_played"))
            {
                var intro = new IntroWindow();
                intro.ShowDialog();
                if (!intro.UserConsented) { Shutdown(); return; }

                var left = intro.Left; var top = intro.Top;
                var clipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "excoworker_clip.mp4");
                if (File.Exists(clipPath))
                {
                    var video = new VideoWindow(clipPath, left, top);
                    await video.PlayAsync();
                }
                _story.SetCheckpoint("video_played");
            }

            var sandboxPath = SandboxHelper.EnsureSandboxFolder();
            ShortcutHelper.CreateShortcutsIfMissing(sandboxPath);
            System.Diagnostics.Process.Start("explorer.exe", sandboxPath);

            _story.SetCheckpoint("tools_opened");

            BackgroundManager.Instance.Start();
            TrayIconManager.CreateTrayIcon();
        }
    }
}
