using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Forms = System.Windows.Forms;
using OOS.Shared;

namespace OOS.Game
{
    internal sealed class BackgroundManager
    {
        private static readonly Lazy<BackgroundManager> _lazy = new(() => new BackgroundManager());
        public static BackgroundManager Instance => _lazy.Value;

        private Forms.NotifyIcon? _tray;
        private bool _running;

        private BackgroundManager() { }

        public void Start()
        {
            if (_running) return;
            _running = true;
            CreateTrayIcon();
            SharedLogger.Info("BackgroundManager started.");
        }

        public void Stop()
        {
            if (!_running) return;
            try { _tray?.Dispose(); } catch { }
            _tray = null;
            _running = false;
            SharedLogger.Info("BackgroundManager stopped.");
        }

        private void CreateTrayIcon()
        {
            _tray = new Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Office of Shadows (running)"
            };

            var menu = new Forms.ContextMenuStrip();

            menu.Items.Add(new Forms.ToolStripMenuItem("Open Workspace", null, (_, __) => OpenFolder(AppPaths.SandboxRoot)));
            menu.Items.Add(new Forms.ToolStripMenuItem("Validate / Repair Files", null, async (_, __) => await RunIntegrityAsync()));
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(new Forms.ToolStripMenuItem("Open Terminal", null, (_, __) => LaunchTool("OOS.Terminal.exe")));
            menu.Items.Add(new Forms.ToolStripMenuItem("Open Device Manager", null, (_, __) => LaunchTool("OOS.DeviceManager.exe")));
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(new Forms.ToolStripMenuItem("Quit", null, (_, __) => Application.Current.Shutdown()));

            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += (_, __) => OpenFolder(AppPaths.SandboxRoot);
        }

        private static void OpenFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                Process.Start(new ProcessStartInfo("explorer.exe")
                {
                    UseShellExecute = true,
                    Arguments = $"\"{path}\""
                });
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"OpenFolder failed: {ex.Message}");
            }
        }

        /// <summary>Public so existing callers (e.g., ToolDeployer/TrayIconManager) can use it.</summary>
        public static void LaunchTool(string exeName)
        {
            try
            {
                // packaged: next to game exe
                var nearGame = Path.Combine(AppPaths.BaseDir, exeName);

                // dev fallback: sibling project's debug output
                var sibling = Path.GetFullPath(Path.Combine(
                    AppPaths.BaseDir, "..", "..", "..",
                    Path.GetFileNameWithoutExtension(exeName)!, "bin", "Debug", "net8.0-windows", exeName));

                var target = File.Exists(nearGame) ? nearGame :
                             File.Exists(sibling) ? sibling : null;

                if (target == null)
                {
                    SharedLogger.Warn($"Tool not found: {exeName}");
                    return;
                }

                Process.Start(new ProcessStartInfo(target)
                {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(target)!
                });
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"LaunchTool({exeName}) failed: {ex.Message}");
            }
        }

        public async Task RunIntegrityAsync()
        {
            try
            {
                Directory.CreateDirectory(AppPaths.IntegrityDir);
                var report = IntegrityManager.ValidateAndRepair(
                    AppPaths.ManifestPath,
                    AppPaths.SandboxRoot,
                    AppPaths.IntegrityDir);

                SharedLogger.Info($"Integrity check complete. Report: {report}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                SharedLogger.Error($"Integrity run failed: {ex.Message}");
            }
        }
    }
}
