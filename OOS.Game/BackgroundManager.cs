using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;

namespace OOS.Game
{
    public sealed class BackgroundManager
    {
        public static BackgroundManager Instance { get; } = new BackgroundManager();

        private System.Timers.Timer? _timer;
        private readonly List<Action> _events = new();
        private int _idx;
        private readonly string _sandbox = SandboxHelper.EnsureSandboxFolder();

        private BackgroundManager() { }

        public void Start()
        {
            // Schedule some example events (replace with your own / JSON-driven)
            _events.Add(() => DropFile("we_are_watching.txt", "we are watching you."));
            _events.Add(() => ShowMessage("Incoming message", "Check the folder..."));
            _events.Add(() => ShowTerminalPopup());

            _timer = new Timer(30_000); // every 30s
            _timer.Elapsed += (s, e) => Application.Current.Dispatcher.Invoke(RunNextEvent);
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        private void RunNextEvent()
        {
            if (_events.Count == 0) return;
            var action = _events[_idx % _events.Count];
            _idx++;
            action();
        }

        public void ResetEverything()
        {
            Stop();
            try
            {
                if (Directory.Exists(_sandbox))
                {
                    Directory.Delete(_sandbox, true);
                    Directory.CreateDirectory(_sandbox);
                }
            }
            catch { /* ignore */ }
            Start();
        }

        private void DropFile(string name, string content)
        {
            var path = Path.Combine(_sandbox, name);
            File.WriteAllText(path, content);
        }

        private void ShowMessage(string title, string body)
        {
            // For dev simplicity; later swap to Windows toasts (Toolkit) if you want
            MessageBox.Show(body, title);
        }

        private static bool LaunchTool(string exeName, params string[] args)
        {
            try
            {
                var path = System.IO.Path.Combine(App.BaseDir, exeName);
                if (!System.IO.File.Exists(path))
                {
                    OOS.Shared.SharedLogger.Warn($"Tool not found: {exeName} (looked in {App.BaseDir})");
                    return false;
                }

                var psi = new System.Diagnostics.ProcessStartInfo(path)
                {
                    UseShellExecute = true,
                    WorkingDirectory = App.BaseDir
                };

                // For .NET 8 you can use ArgumentList; with UseShellExecute=true, Args still work.
                if (args is { Length: > 0 })
                    psi.Arguments = string.Join(" ", args);

                System.Diagnostics.Process.Start(psi);
                OOS.Shared.SharedLogger.Info($"Launched tool: {path}");
                return true;
            }
            catch (Exception ex)
            {
                OOS.Shared.SharedLogger.Warn($"Failed to launch tool '{exeName}': {ex.Message}");
                return false;
            }
        }



        private void ShowTerminalPopup()
        {
            try
            {
                _ = LaunchTool("OOS.Terminal.exe"); // ignore return, or handle if you want
            }
            catch { /* no-op; we already log inside LaunchTool */ }
        }

    }
}
