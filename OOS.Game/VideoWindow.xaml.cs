using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace OOS.Game
{
    public partial class VideoWindow : Window
    {
        public VideoWindow(string fileName)
        {
            InitializeComponent();

            string path = ResolveVideoPath(fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Video not found", path);

            mediaElement.Source = new Uri(path);

            // Slightly larger than your intro dialog (feel free to tweak):
            Width = 980;
            Height = 620;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            mediaElement.MediaEnded += MediaElement_MediaEnded;
            mediaElement.MediaFailed += MediaElement_MediaFailed;
            Loaded += (_, __) => mediaElement.Play();
        }

        private static string ResolveVideoPath(string fileName)
        {
            var p1 = Path.Combine(AppPaths.VideosDir, fileName);
            if (File.Exists(p1)) return p1;

            var p2 = Path.Combine(AppPaths.AssetsDir, fileName);
            if (File.Exists(p2)) return p2;

            var p3 = Path.Combine(Environment.CurrentDirectory, fileName);
            return p3;
        }

        private void MediaElement_MediaEnded(object? sender, RoutedEventArgs e) => Close();

        private void MediaElement_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Failed to load video: {e.ErrorException?.Message}", "Video Error");
            Close();
        }
    }
}
