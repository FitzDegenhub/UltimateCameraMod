using System.Runtime.InteropServices;

namespace UltimateCameraMod.V3;

/// <summary>
/// Sets <see cref="ApplicationIdentity.AppUserModelId"/> and icon on the HWND property store.
/// The taskbar "group" icon (especially when buttons are combined) is driven by
/// <c>System.AppUserModel.RelaunchIconResource</c>, which Microsoft documents as applying only when
/// <c>System.AppUserModel.ID</c> is set on the <b>window</b> via <c>SHGetPropertyStoreForWindow</c> —
/// not merely via <c>SetCurrentProcessExplicitAppUserModelID</c>.
/// </summary>
internal static class ShellTaskbarPropertyStore
{
    private static readonly Guid IID_IPropertyStore = new("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99");

    /// <summary>PKEY_AppUserModel_ID — fmtid + pid 5.</summary>
    private static readonly PROPERTYKEY PKEY_AppUserModel_ID = new(
        new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

    /// <summary>PKEY_AppUserModel_RelaunchIconResource — same fmtid, pid 3.</summary>
    private static readonly PROPERTYKEY PKEY_AppUserModel_RelaunchIconResource = new(
        new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);

    private const int S_OK = 0;
    private const ushort VT_LPWSTR = 31;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;

        public PROPERTYKEY(Guid fmtid, uint pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }
    }

    /// <summary>Layout must match Win32 <c>PROPVARIANT</c> for <c>VT_LPWSTR</c> (first pointer in the union).</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    private struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr ptr;
        public IntPtr data2;
    }

    [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out uint cProps);

        [PreserveSig]
        int GetAt(uint iProp, out PROPERTYKEY key);

        [PreserveSig]
        int GetValue(in PROPERTYKEY key, out PROPVARIANT pv);

        [PreserveSig]
        int SetValue(in PROPERTYKEY key, in PROPVARIANT propvar);

        [PreserveSig]
        int Commit();
    }

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, in Guid riid, [MarshalAs(UnmanagedType.Interface)] out IPropertyStore? ppv);

    /// <summary>
    /// Builds a <see cref="VT_LPWSTR"/> variant without <c>InitPropVariantFromString</c>, which is missing from
    /// some Propsys.dll builds (EntryPointNotFoundException on certain Windows SKUs / forwarders).
    /// </summary>
    private static PROPVARIANT CreateStringPropVariant(string value)
    {
        IntPtr p = Marshal.StringToCoTaskMemUni(value);
        return new PROPVARIANT
        {
            vt = VT_LPWSTR,
            wReserved1 = 0,
            wReserved2 = 0,
            wReserved3 = 0,
            ptr = p,
            data2 = IntPtr.Zero
        };
    }

    /// <summary>
    /// Frees a <see cref="CreateStringPropVariant"/> buffer without <c>PropVariantClear</c> (also missing from some Propsys builds).
    /// </summary>
    private static void FreeOwnedStringPropVariant(ref PROPVARIANT pv)
    {
        if (pv.vt == VT_LPWSTR && pv.ptr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(pv.ptr);
            pv.ptr = IntPtr.Zero;
            pv.vt = 0;
        }
    }

    /// <summary>Absolute path to .ico for RelaunchIconResource, or null to skip icon property.</summary>
    public static string? ResolveIconPathForShell(string exeDir)
    {
        string assets = Path.Combine(exeDir, "Assets", "ucm.ico");
        if (File.Exists(assets))
            return Path.GetFullPath(assets);
        string root = Path.Combine(exeDir, "ucm.ico");
        if (File.Exists(root))
            return Path.GetFullPath(root);
        return null;
    }

    /// <returns>true if AUMID was applied on the window (icon optional).</returns>
    public static bool TryApply(IntPtr hwnd, string exeDir)
    {
        if (hwnd == IntPtr.Zero)
            return false;

        int hr = SHGetPropertyStoreForWindow(hwnd, in IID_IPropertyStore, out IPropertyStore? store);
        if (hr != S_OK || store == null)
            return false;

        PROPVARIANT pvId = CreateStringPropVariant(ApplicationIdentity.AppUserModelId);
        try
        {
            hr = store.SetValue(in PKEY_AppUserModel_ID, in pvId);
            if (hr != S_OK)
                return false;
        }
        finally
        {
            FreeOwnedStringPropVariant(ref pvId);
        }

        string? ico = ResolveIconPathForShell(exeDir);
        if (!string.IsNullOrEmpty(ico))
        {
            // Docs: single-image .ico → ",0" (no hyphen). Path must be absolute.
            string spec = ico + ",0";
            PROPVARIANT pvIcon = CreateStringPropVariant(spec);
            try
            {
                store.SetValue(in PKEY_AppUserModel_RelaunchIconResource, in pvIcon);
            }
            finally
            {
                FreeOwnedStringPropVariant(ref pvIcon);
            }
        }

        return true;
    }

    /// <summary>MSFT: clear window shell props before close to release resources.</summary>
    public static void TryClear(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            return;

        int hr = SHGetPropertyStoreForWindow(hwnd, in IID_IPropertyStore, out IPropertyStore? store);
        if (hr != S_OK || store == null)
            return;

        var empty = new PROPVARIANT { vt = 0 };
        store.SetValue(in PKEY_AppUserModel_RelaunchIconResource, in empty);
        store.SetValue(in PKEY_AppUserModel_ID, in empty);
    }
}
