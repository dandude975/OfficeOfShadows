using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using OOS.Shared;

namespace OOS.Game
{
    public sealed class BackgroundManager
    {
        private static readonly Lazy<BackgroundManager> _lazy = new(() => new BackgroundManager());
        public static BackgroundManager Instance => _lazy.Value;

        private Timer? _timer;

        private BackgroundManager() { }

        public void Start()
        {
            if (_timer != null) return;
            _timer = new Timer(30000); // every 30s, placeholder for future events
            _timer.Elapsed += (_, __) => { /* future: popups, hints, etc. */ };
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// Launch a tool by exe name (relative to game BaseDir) or by absolute path.
        /// </summary>
        public static void LaunchTool(string exeOrPath)
        {
            try
            {
                string path = exeOrPath;
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(AppPaths.BaseDir, exeOrPath);

                if (!File.Exists(path))
                {
                    SharedLogger.Warn($"Tool not found: {path}");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Failed to launch tool '{exeOrPath}': {ex.Message}");
            }
        }
    }
}
