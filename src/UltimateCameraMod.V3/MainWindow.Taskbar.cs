using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UltimateCameraMod.V3.Controls;
using UltimateCameraMod.V3.Localization;
using UltimateCameraMod.V3.Models;
using UltimateCameraMod.Models;
using UltimateCameraMod.Services;

namespace UltimateCameraMod.V3;

public partial class MainWindow : Window
{
    private const int WM_SETICON = 0x0080;
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int ICON_SMALL2 = 2;
    private const uint IMAGE_ICON = 1;
    private const uint LR_DEFAULTCOLOR = 0;
    private const uint LR_LOADFROMFILE = 0x00000010;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr LoadImage(IntPtr hInst, IntPtr lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private const int GCLP_HICON = -14;
    private const int GCLP_HICONSM = -34;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static readonly object TaskbarIconLock = new();
    private static IntPtr _hIcon16;
    private static IntPtr _hIcon24;
    private static IntPtr _hIconBig;

    private static IEnumerable<string> EnumerateUcmIcoFilePaths()
    {
        string assets = Path.Combine(ExeDir, "Assets", "ucm.ico");
        string root = Path.Combine(ExeDir, "ucm.ico");
        if (File.Exists(assets)) yield return assets;
        if (File.Exists(root) && !string.Equals(assets, root, StringComparison.OrdinalIgnoreCase))
            yield return root;
    }

    private static void EnsureTaskbarIconAssets()
    {
        lock (TaskbarIconLock)
        {
            if (_hIcon16 != IntPtr.Zero && _hIcon24 != IntPtr.Zero && _hIconBig != IntPtr.Zero)
                return;

            IntPtr mod = GetModuleHandle(null);
            if (_hIcon16 == IntPtr.Zero)
                _hIcon16 = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR);
            if (_hIcon24 == IntPtr.Zero)
                _hIcon24 = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 24, 24, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 48, 48, LR_DEFAULTCOLOR);
            if (_hIconBig == IntPtr.Zero)
                _hIconBig = LoadImage(mod, (IntPtr)1, IMAGE_ICON, 256, 256, LR_DEFAULTCOLOR);

            foreach (string icoPath in EnumerateUcmIcoFilePaths())
            {
                if (_hIcon16 == IntPtr.Zero)
                    _hIcon16 = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 16, 16, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIcon24 == IntPtr.Zero)
                    _hIcon24 = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 24, 24, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 32, 32, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 48, 48, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIconBig == IntPtr.Zero)
                    _hIconBig = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 256, 256, LR_DEFAULTCOLOR | LR_LOADFROMFILE);
                if (_hIcon16 != IntPtr.Zero && _hIcon24 != IntPtr.Zero && _hIconBig != IntPtr.Zero)
                    break;
            }

            if (_hIcon24 == IntPtr.Zero && _hIcon16 != IntPtr.Zero)
                _hIcon24 = _hIcon16;
            if (_hIconBig == IntPtr.Zero && _hIcon16 != IntPtr.Zero)
                _hIconBig = _hIcon16;
        }
    }

    private void OnMainWindowContentRendered(object? sender, EventArgs e)
    {
        if (_taskbarIconContentRenderedDone) return;
        _taskbarIconContentRenderedDone = true;
        ApplyNativeWindowIcons();
    }

    private void OnMainWindowFirstActivated(object? sender, EventArgs e)
    {
        if (_taskbarIconActivatedDone) return;
        _taskbarIconActivatedDone = true;
        ApplyNativeWindowIcons();
        Activated -= OnMainWindowFirstActivated;
    }

    private void OnMainWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool vis && vis && IsLoaded)
            ApplyNativeWindowIcons();
    }

    private void ScheduleTaskbarIconDelayedRetries()
    {
        void Kick(int ms)
        {
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (_, _) =>
            {
                t.Stop();
                ApplyNativeWindowIcons();
            };
            t.Start();
        }

        Kick(120);
        Kick(400);
        Kick(1200);
        Kick(2500);
        Kick(4500);
    }

    private void ApplyNativeWindowIcons()
    {
        try
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            EnsureTaskbarIconAssets();

            IntPtr mid = _hIcon24 != IntPtr.Zero ? _hIcon24 : _hIcon16;
            if (_hIcon16 != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, _hIcon16);
            if (mid != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_SMALL2, mid);
            if (_hIconBig != IntPtr.Zero)
                SendMessage(hwnd, WM_SETICON, (IntPtr)ICON_BIG, _hIconBig);

            // Some shell paths read class icons on cold start; mirror WM_SETICON here.
            if (_hIconBig != IntPtr.Zero)
                SetClassLongPtr(hwnd, GCLP_HICON, _hIconBig);
            if (_hIcon16 != IntPtr.Zero)
                SetClassLongPtr(hwnd, GCLP_HICONSM, _hIcon16);
        }
        catch
        {
            // Non-fatal: title bar pack URI icon still applies where supported.
        }
    }

}
