using System;
using System.Threading.Tasks;
using System.Windows;

namespace OOS.Game
{
    public partial class VideoWindow : Window
    {
        private TaskCompletionSource<bool>? _tcs;

        // Desired size bounds (tweak to taste)
        private const double MIN_W = 880;  // a touch bigger than the intro
        private const double MIN_H = 540;
        private const double MAX_W = 1100; // cap so huge videos don't go full screen
        private const double MAX_H = 700;

        public VideoWindow(string absolutePath)
        {
            InitializeComponent();

            video.Source = new Uri(absolutePath, UriKind.Absolute);

            video.MediaOpened += (s, e) =>
            {
                // Natural size of the media
                int natW = video.NaturalVideoWidth;
                int natH = video.NaturalVideoHeight;

                if (natW > 0 && natH > 0)
                {
                    // scale uniformly within our caps
                    double scaleW = MAX_W / natW;
                    double scaleH = MAX_H / natH;
                    double scale = Math.Min(Math.Min(scaleW, scaleH), 1.0); // never upscale above natural size unless below mins

                    double targetW = natW * scale;
                    double targetH = natH * scale;

                    // ensure minimum window size for consistent look
                    targetW = Math.Max(targetW, MIN_W);
                    targetH = Math.Max(targetH, MIN_H);

                    // We size the MediaElement; window (SizeToContent) will wrap it
                    video.Width = targetW;
                    video.Height = targetH;
                }
            };

            video.MediaEnded += (s, e) => _tcs?.TrySetResult(true);
            video.MediaFailed += (s, e) =>
            {
                MessageBox.Show(
                    $"Failed to play video:\n{absolutePath}\n\n{e.ErrorException?.Message}",
                    "Office of Shadows");
                _tcs?.TrySetResult(true);
            };
        }

        public Task PlayAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            Show();
            video.Play();
            return _tcs.Task.ContinueWith(_ => Dispatcher.Invoke(Close));
        }
    }
}
