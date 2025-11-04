using System.Windows;

namespace OOS.Game
{
    public partial class IntroWindow : Window
    {
        public bool UserConsented { get; private set; }

        public IntroWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AcknowledgeCheck.IsChecked == true)
            {
                UserConsented = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please confirm you understand before continuing.", "Office of Shadows");
            }
        }
    }
}
