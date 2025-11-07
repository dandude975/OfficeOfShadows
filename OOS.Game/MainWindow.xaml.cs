using System.Windows;

namespace OOS.Game
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            var exe = System.IO.Path.Combine(App.BaseDir, "OOS.Terminal.exe");
            if (System.IO.File.Exists(exe))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe)
                {
                    UseShellExecute = true
                });
            }
            else
            {
                OOS.Shared.SharedLogger.Warn("Terminal EXE not found.");
            }
        }

    }
}
