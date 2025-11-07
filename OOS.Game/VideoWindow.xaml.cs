using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace OOS.Game
{
    public partial class VideoWindow : Window
    {
        private readonly string _path;

        public VideoWindow(string fileName)
        {
            InitializeComponent();
            _path = Path.Combine(App.AssetsDir, "Videos", fileName);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_path)) { Close(); return; }
            mediaElement.Source = new Uri(_path);
            mediaElement.Play();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e) => Close();

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Video error: {e.ErrorException.Message}", "Video", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape) Close();
        }
    }
}
