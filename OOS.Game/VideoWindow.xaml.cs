using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace OOS.Game
{
    public partial class VideoWindow : Window
    {
        private readonly string _path;

        public VideoWindow(string absoluteVideoPath)
        {
            InitializeComponent();
            _path = absoluteVideoPath;
            Loaded += (_, __) => PlayNow();
        }

        private void PlayNow()
        {
            try
            {
                if (!File.Exists(_path))
                    throw new FileNotFoundException("video not found", _path);

                media.Source = new Uri(_path, UriKind.Absolute);
                media.MediaEnded += (_, __) => Close();
                media.MediaFailed += (_, e) =>
                {
                    MessageBox.Show($"Failed to load video: {e.ErrorException?.Message}", "Video Error");
                    Close();
                };
                media.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Video Error");
                Close();
            }
        }
    }
}
