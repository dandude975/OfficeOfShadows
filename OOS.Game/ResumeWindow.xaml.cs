using System;
using System.IO;
using System.Windows;
using OOS.Shared;

namespace OOS.Game
{
    public partial class ResumeWindow : Window
    {
        private readonly string _savePath;

        public ResumeWindow()
        {
            InitializeComponent();
            _savePath = Path.Combine(AppPaths.SaveDir, "save.json");

            if (File.Exists(_savePath))
            {
                var save = GameSave.Load(_savePath);
                lblSaveInfo.Text = $"{save.Checkpoint} — {save.TimestampUtc.ToLocalTime():g}";


            }
            else
            {
                lblSaveInfo.Text = "No save found.";
            }
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            // Play the resume clip (or jump right into desktop if you prefer)
            var video = new VideoWindow("resume.mp4");
            video.ShowDialog();
            Close();
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_savePath))
                    File.Delete(_savePath);
            }
            catch { /* ignore */ }

            var video = new VideoWindow("intro.mp4");
            video.ShowDialog();
            Close();
        }
    }
}
