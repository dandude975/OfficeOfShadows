using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using OOS.Shared;
using OOS.DeviceManager.Services;

namespace OOS.DeviceManager
{
    public partial class MainWindow : Window
    {
        private readonly DeviceService _svc;
        private List<DeviceInfo> _devices = new();

        public MainWindow()
        {
            InitializeComponent();
            _svc = new DeviceService(AppDomain.CurrentDomain.BaseDirectory);
            _ = LoadDevicesAsync(false);
        }

        private async Task LoadDevicesAsync(bool probeLan)
        {
            DeviceList.ItemsSource = null;
            _devices = await _svc.GetMergedDevicesAsync(doLanProbe: probeLan);
            DeviceList.ItemsSource = _devices;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e) =>
            await LoadDevicesAsync(false);

        private async void ProbeLan_Click(object sender, RoutedEventArgs e) =>
            await LoadDevicesAsync(true);

        private void DeviceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DeviceList.SelectedItem is DeviceInfo d)
            {
                DetailsTitle.Text = d.Name;
                Details.Text =
                    $"Type: {d.Type}\n" +
                    $"Manufacturer: {d.Manufacturer}\n" +
                    $"IP: {d.Ip}\n" +
                    $"MAC: {d.Mac}\n" +
                    $"Notes: {d.Notes}\n" +
                    $"Fake: {d.IsFake}\n" +
                    $"Metadata:\n{(d.Metadata == null ? "(none)" : string.Join("\n", d.Metadata.Select(kv => $"{kv.Key}: {kv.Value}")))}";
            }
        }

        private void SimDisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem is DeviceInfo d)
            {
                // Send a shared message so EventEngine can react
                FileQueue.Enqueue(new GameMessage
                {
                    Type = "device.disconnect_attempt",
                    From = "DeviceManager",
                    Data = new { id = d.Id, name = d.Name, isFake = d.IsFake }
                });

                MessageBox.Show($"Simulated disconnect (event sent): {d.Name}", "Device Manager");
            }
        }

        private void MarkSuspiciousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem is DeviceInfo d)
            {
                FileQueue.Enqueue(new GameMessage
                {
                    Type = "device.mark_suspicious",
                    From = "DeviceManager",
                    Data = new { id = d.Id, name = d.Name }
                });

                MessageBox.Show($"Marked suspicious: {d.Name}", "Device Manager");
            }
        }
    }
}
