// OOS.Shared/SharedLogger.cs
using System;
using System.IO;

// OOS.Shared/SharedLogger.cs
namespace OOS.Shared
{
    public static class SharedLogger
    {
        public static void Info(string m) { System.Diagnostics.Debug.WriteLine("[INFO] " + m); }
        public static void Warn(string m) { System.Diagnostics.Debug.WriteLine("[WARN] " + m); }
        public static void Error(string m) { System.Diagnostics.Debug.WriteLine("[ERR ] " + m); }
    }
}