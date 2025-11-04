using System;
using System.Threading.Tasks;
using System.Windows;

namespace OOS.Game
{
    public partial class VideoWindow : Window
    {
        private TaskCompletionSource<bool> _tcs;

        public VideoWindow(string relativePath)
        {
            InitializeComponent();
            video.Source = new Uri(System.IO.Path.GetFullPath(relativePath));
            video.MediaEnded += (s, e) => _tcs?.TrySetResult(true);
        }

        public Task PlayAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            Show();
            video.Play();
            return _tcs.Task.ContinueWith(t => { Dispatcher.Invoke(Close); });
        }
    }
}
