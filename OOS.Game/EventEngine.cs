using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OOS.Game
{
    /// <summary>
    /// Persistent background engine that simulates real-time in-game events.
    /// Runs silently after startup to monitor and trigger story or environmental changes.
    /// </summary>
    public class EventEngine
    {
        private CancellationTokenSource _cts;
        private readonly string _sandboxPath;
        private readonly string _logPath;
        private readonly Random _rng = new();

        /// <summary>
        /// Creates a new EventEngine instance.
        /// </summary>
        public EventEngine()
        {
            _sandboxPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Office Work Stuff");

            _logPath = Path.Combine(
                AppContext.BaseDirectory,
                "FileValidation",
                "event_engine.log");

            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        }

        /// <summary>
        /// Starts the event engine loop asynchronously.
        /// </summary>
        public void Start()
        {
            if (_cts != null)
                return; // already running

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Log("[EventEngine] Started background system.");

            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // Poll every 5–10 seconds (random to feel organic)
                        await Task.Delay(TimeSpan.FromSeconds(_rng.Next(5, 11)), token);

                        // Placeholder background logic:
                        // Check sandbox integrity, simulate network events, etc.
                        RunBackgroundCheck();

                        // Occasionally trigger fake in-game "system messages"
                        if (_rng.NextDouble() < 0.2) // 20% chance
                            TriggerSystemEvent();
                    }
                }
                catch (TaskCanceledException)
                {
                    Log("[EventEngine] Gracefully stopped (task canceled).");
                }
                catch (Exception ex)
                {
                    Log($"[EventEngine] ERROR: {ex.Message}");
                }
            }, token);
        }

        /// <summary>
        /// Stops the event engine safely.
        /// </summary>
        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _cts = null;
                Log("[EventEngine] Stopped background system.");
            }
            catch (Exception ex)
            {
                Log($"[EventEngine] Stop error: {ex.Message}");
            }
        }

        /// <summary>
        /// Simple placeholder that checks sandbox for missing or new files.
        /// </summary>
        private void RunBackgroundCheck()
        {
            try
            {
                if (!Directory.Exists(_sandboxPath))
                    return;

                var files = Directory.GetFiles(_sandboxPath);
                if (files.Length == 0)
                {
                    Log("[Sandbox Monitor] No files detected in workspace.");
                }
                else
                {
                    Log($"[Sandbox Monitor] {files.Length} items currently in workspace.");
                }
            }
            catch (Exception ex)
            {
                Log($"[Sandbox Monitor] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Placeholder to trigger fake “system” events for later expansion.
        /// </summary>
        private void TriggerSystemEvent()
        {
            string[] fakeMessages =
            {
                "Background sync complete.",
                "Network latency spike detected.",
                "VPN handshake refreshed.",
                "Unauthorized script attempt logged.",
                "File validation daemon restarted."
            };

            string message = fakeMessages[_rng.Next(fakeMessages.Length)];
            Log($"[System Event] {message}");
        }

        /// <summary>
        /// Logs background messages to a file in /FileValidation for debugging or narrative trace.
        /// </summary>
        private void Log(string message)
        {
            try
            {
                string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
                File.AppendAllText(_logPath, line + Environment.NewLine);
                Console.WriteLine(line); // visible in debug output
            }
            catch { /* ignore logging errors */ }
        }
    }
}
