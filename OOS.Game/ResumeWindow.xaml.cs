using System;
using System.IO;
using System.Windows;
using OOS.Shared;

namespace OOS.Game
{
    public partial class ResumeWindow : Window
    {
        public bool ContinueChosen { get; private set; }
        public bool NewGameChosen { get; private set; }

        private readonly string _savePath = AppPaths.SavePath("save.json");

        public ResumeWindow()
        {
            InitializeComponent();

            var save = GameSave.TryLoad(_savePath);
            if (save == null)
            {
                lblSaveInfo.Text = "No previous save found.";
                ContinueButton.IsEnabled = false;
            }
            else
            {
                lblSaveInfo.Text = $"{save.Checkpoint} — {save.TimestampUtc.ToLocalTime():g}";
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueChosen = true;
            DialogResult = true;
            Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGameChosen = true;
            DialogResult = true;
            Close();
        }
    }
}
