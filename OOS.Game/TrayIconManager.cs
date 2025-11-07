using System;
using System.Windows.Forms;
using OOS.Shared;

namespace OOS.Game
{
    /// <summary>
    /// Simple tray icon with a quit and a “launch tools” action.
    /// No BackgroundManager.Instance – we just call static helpers.
    /// </summary>
    public sealed class TrayIconManager : IDisposable
    {
        private NotifyIcon? _tray;

        public void CreateTrayIcon()
        {
            _tray = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Office of Shadows (running)"
            };

            var menu = new ContextMenuStrip();

            var openTerminal = new ToolStripMenuItem("Open Terminal");
            openTerminal.Click += (_, __) => BackgroundManager.LaunchTool("OOS.Terminal.exe");
            menu.Items.Add(openTerminal);

            var quit = new ToolStripMenuItem("Quit");
            quit.Click += (_, __) => System.Windows.Application.Current.Shutdown();
            menu.Items.Add(quit);

            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += (_, __) => MessageBox.Show("Office of Shadows is running in the background.", "Office of Shadows");
        }

        public void Dispose()
        {
            try
            {
                if (_tray != null)
                {
                    _tray.Visible = false;
                    _tray.Dispose();
                    _tray = null;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.Warn($"Tray dispose failed: {ex.Message}");
            }
        }
    }
}
