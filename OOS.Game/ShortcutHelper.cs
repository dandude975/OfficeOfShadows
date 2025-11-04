using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OOS.Game
{
    public static class ShortcutHelper
    {
        public static void CreateShortcutsIfMissing(string targetFolder)
        {
            var gameBase = AppDomain.CurrentDomain.BaseDirectory;

            // EXAMPLES — point these at your built exes (dist or debug)
            var terminalExe = Path.GetFullPath(
                Path.Combine(gameBase, @"..\..\OOS.Terminal\bin\Debug\net8.0-windows\OOS.Terminal.exe"));

            TryCreateShortcut(targetFolder, "Terminal", terminalExe);
            // Add more:
            // var vpnExe = ...
            // TryCreateShortcut(targetFolder, "VPN", vpnExe);
        }

        private static void TryCreateShortcut(string folder, string name, string exePath)
        {
            if (!File.Exists(exePath)) return;

            Directory.CreateDirectory(folder);
            var linkPath = Path.Combine(folder, $"{name}.lnk");
            if (File.Exists(linkPath)) return;

            CreateShellLink(linkPath,
                            exePath,
                            Path.GetDirectoryName(exePath)!,
                            "",                   // args
                            $"Open {name}",       // description
                            exePath, 0);          // icon (path,index)
        }

        // ---------- Shell Link COM interop (no external references) ----------

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink { }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            [PreserveSig] int IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        private static void CreateShellLink(string linkPath, string targetPath, string workingDir,
                                            string arguments, string description, string iconPath, int iconIndex = 0)
        {
            var link = (IShellLinkW)new ShellLink();
            link.SetPath(targetPath);
            link.SetWorkingDirectory(workingDir);
            if (!string.IsNullOrEmpty(arguments)) link.SetArguments(arguments);
            if (!string.IsNullOrEmpty(description)) link.SetDescription(description);
            if (!string.IsNullOrEmpty(iconPath)) link.SetIconLocation(iconPath, iconIndex);

            var file = (IPersistFile)link;
            file.Save(linkPath, true);
        }
    }
}
