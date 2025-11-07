// OOS.DeviceManager/Services/DeviceService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;          // ← this one comes from System.Management
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;
using OOS.Shared;                 // ← this comes from the project reference


namespace OOS.DeviceManager.Services
{
    public class DeviceService
    {
        private readonly string _fakePath;

        public DeviceService(string assetsFolder)
        {
            _fakePath = Path.Combine(assetsFolder ?? AppDomain.CurrentDomain.BaseDirectory, "Assets", "fake_devices.json");
        }

        /// <summary>
        /// Main entry: gets merged list of real + fake devices.
        /// </summary>
        public async Task<List<DeviceInfo>> GetMergedDevicesAsync(bool doLanProbe = false)
        {
            var real = new List<DeviceInfo>();

            // WMI queries (safe read-only)
            real.AddRange(GetPrinters());
            real.AddRange(GetMonitors());
            real.AddRange(GetUsbPnPDevices());
            real.AddRange(GetDisks());
            real.AddRange(GetPhysicalMemory());
            real.AddRange(GetCpu());
            real.AddRange(GetGpus());
            real.AddRange(GetAudioDevices());
            real.AddRange(GetNetworkAdapters());

            // Optionally probe LAN for live addresses (slow)
            if (doLanProbe)
            {
                var lan = await ProbeLanAsync();
                real.AddRange(lan);
            }

            var fake = LoadFakeDevices();

            // Merge by Id or by IP/MAC if Id collides
            var merged = new Dictionary<string, DeviceInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in real)
            {
                var key = r.Id ?? $"{r.Type}:{r.Mac ?? r.Ip ?? r.Name}";
                if (!merged.ContainsKey(key)) merged[key] = r;
            }

            foreach (var f in fake)
            {
                // prefer fake item id if unique; ensure not to overwrite real devices accidentally
                if (!merged.ContainsKey(f.Id))
                {
                    merged[f.Id] = f;
                }
                else
                {
                    // collision: add suffix to fake id to keep both visible
                    var newId = f.Id + "-fake";
                    f.Id = newId;
                    merged[newId] = f;
                }
            }

            return merged.Values.OrderBy(d => d.IsFake).ThenBy(d => d.Type).ThenBy(d => d.Name).ToList();
        }

        #region Fake loader
        private List<DeviceInfo> LoadFakeDevices()
        {
            try
            {
                if (!File.Exists(_fakePath)) return new List<DeviceInfo>();
                var json = File.ReadAllText(_fakePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<DeviceInfo>>(json, opts);
                if (list == null) return new List<DeviceInfo>();
                foreach (var i in list) i.IsFake = true;
                return list;
            }
            catch
            {
                return new List<DeviceInfo>();
            }
        }
        #endregion

        #region WMI / System probes (read-only)

        private IEnumerable<DeviceInfo> GetPrinters()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["DeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "printer",
                        Name = (mo["Name"] ?? "").ToString(),
                        Manufacturer = (mo["DriverName"] ?? "").ToString(),
                        Path = mo["DeviceID"]?.ToString() ?? "",
                        IsFake = false,
                        Notes = (mo["PortName"] ?? "").ToString()
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetMonitors()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["PNPDeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "monitor",
                        Name = (mo["Name"] ?? "Monitor").ToString(),
                        Manufacturer = (mo["MonitorManufacturer"] ?? "").ToString(),
                        Path = mo["PNPDeviceID"]?.ToString() ?? "",
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetUsbPnPDevices()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%'");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["DeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "usb",
                        Name = (mo["Name"] ?? "USB Device").ToString(),
                        Manufacturer = (mo["Manufacturer"] ?? "").ToString(),
                        Path = mo["DeviceID"]?.ToString() ?? "",
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetDisks()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["DeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "disk",
                        Name = (mo["Model"] ?? "Disk").ToString(),
                        Manufacturer = (mo["Manufacturer"] ?? "").ToString(),
                        Path = mo["DeviceID"]?.ToString() ?? "",
                        IsFake = false,
                        Metadata = new Dictionary<string, string>
                        {
                            ["Size"] = mo["Size"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetPhysicalMemory()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["Tag"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "ram",
                        Name = (mo["PartNumber"] ?? "RAM").ToString(),
                        Manufacturer = (mo["Manufacturer"] ?? "").ToString(),
                        Metadata = new Dictionary<string, string>
                        {
                            ["Capacity"] = mo["Capacity"]?.ToString() ?? "",
                            ["Speed"] = mo["Speed"]?.ToString() ?? ""
                        },
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetCpu()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["DeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "cpu",
                        Name = (mo["Name"] ?? "CPU").ToString(),
                        Manufacturer = (mo["Manufacturer"] ?? "").ToString(),
                        Metadata = new Dictionary<string, string>
                        {
                            ["Cores"] = mo["NumberOfCores"]?.ToString() ?? "",
                            ["LogicalProcessors"] = mo["NumberOfLogicalProcessors"]?.ToString() ?? ""
                        },
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetGpus()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["PNPDeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "gpu",
                        Name = (mo["Name"] ?? "GPU").ToString(),
                        Manufacturer = (mo["AdapterCompatibility"] ?? "").ToString(),
                        Metadata = new Dictionary<string, string>
                        {
                            ["DriverVersion"] = mo["DriverVersion"]?.ToString() ?? ""
                        },
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetAudioDevices()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");
                foreach (ManagementObject mo in searcher.Get())
                {
                    outp.Add(new DeviceInfo
                    {
                        Id = mo["DeviceID"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Type = "audio",
                        Name = (mo["Name"] ?? "Audio").ToString(),
                        Manufacturer = (mo["Manufacturer"] ?? "").ToString(),
                        IsFake = false
                    });
                }
            }
            catch { }
            return outp;
        }

        private IEnumerable<DeviceInfo> GetNetworkAdapters()
        {
            var outp = new List<DeviceInfo>();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var ip = ni.GetIPProperties().UnicastAddresses
                               .FirstOrDefault(u => u.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                               .Address.ToString();

                    var mac = string.Join(":", ni.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));

                    outp.Add(new DeviceInfo
                    {
                        Id = ni.Id,
                        Type = "network-adapter",
                        Name = ni.Name,
                        Manufacturer = ni.Description,
                        Mac = mac,
                        Ip = ip,
                        IsNetwork = true,
                        IsFake = false,
                        Metadata = new Dictionary<string, string>
                        {
                            ["Status"] = ni.OperationalStatus.ToString(),
                            ["Speed"] = ni.Speed.ToString()
                        }
                    });
                }
            }
            catch { }
            return outp;
        }

        #endregion

        #region LAN probe (optional, slow)

        /// <summary>
        /// Quick ARP parse + light ping sweep for live LAN entries; keep it optional.
        /// </summary>
        private async Task<List<DeviceInfo>> ProbeLanAsync()
        {
            var devices = new List<DeviceInfo>();

            try
            {
                // parse arp table (soft)
                var arpOutput = RunProcessCapture("arp", "-a");
                if (!string.IsNullOrWhiteSpace(arpOutput))
                {
                    foreach (var line in arpOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // crude parse typical Windows arp output lines:
                        //   192.168.0.1          00-11-22-33-44-55     dynamic
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && parts[0].Contains(".") && parts[1].Contains("-"))
                        {
                            var ip = parts[0];
                            var mac = parts[1].Replace('-', ':').ToUpperInvariant();
                            devices.Add(new DeviceInfo
                            {
                                Id = $"lan-{ip}",
                                Type = "network-device",
                                Name = ip,
                                IsNetwork = true,
                                Ip = ip,
                                Mac = mac,
                                IsFake = false
                            });
                        }
                    }
                }

                // optional: light ping pass on local subnet (commented—slow)
                // you can implement configurable probing later
            }
            catch { }

            return devices;
        }

        private static string RunProcessCapture(string cmd, string args)
        {
            try
            {
                using var p = new Process();
                p.StartInfo.FileName = cmd;
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                var outp = p.StandardOutput.ReadToEnd();
                p.WaitForExit(2000);
                return outp;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
