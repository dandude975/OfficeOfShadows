using System;
using System.IO;
using System.Text.Json;
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
            _savePath = System.IO.Path.Combine(App.SandboxRoot, "save.json");
            LoadSaveData();
        }

        private void LoadSaveData()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    lblSaveInfo.Text = "No previous save found.";
                    return;
                }

                string json = File.ReadAllText(_savePath);
                var save = JsonSerializer.Deserialize<GameSave>(json);

                lblSaveInfo.Text = $"Last saved: {save.Timestamp}\nLocation: {save.Checkpoint}";
            }
            catch (Exception ex)
            {
                lblSaveInfo.Text = $"Error loading save: {ex.Message}";
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            var video = new VideoWindow("resume.mp4");
            video.ShowDialog();
            Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(_savePath))
                File.Delete(_savePath);

            var video = new VideoWindow("intro.mp4");
            video.ShowDialog();
            Close();
        }
    }

    
}
