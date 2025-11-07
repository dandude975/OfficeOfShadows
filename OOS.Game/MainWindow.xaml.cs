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
            // Launch the terminal app (shortcut exists in the sandbox; this opens the EXE directly)
            BackgroundManager.LaunchTool("OOS.Terminal.exe");
        }
    }
}
