// OOS.Shared/Models/DeviceInfo.cs
using System;
using System.Collections.Generic;

namespace OOS.Shared
{
    public class DeviceInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // unique id
        public string Type { get; set; } = "";     // "printer","monitor","usb","ram","disk","cpu","gpu","audio","network-device","router"
        public string Name { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Path { get; set; } = "";     // device instance id or file path (optional)
        public bool IsNetwork { get; set; } = false;
        public string? Ip { get; set; }
        public string? Mac { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? Notes { get; set; }
        public Dictionary<string, string>? Metadata { get; set; } = new();
        public bool IsFake { get; set; } = false;  // useful to highlight/flag
    }
}
