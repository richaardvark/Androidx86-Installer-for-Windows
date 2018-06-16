using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    static class UEFIWrapper
    {
        static IntPtr libHandle;

        #region "Native Functions"
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_Init();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr UEFI_GetBootList();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr[][] UEFI_GetBootDevices();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_isUEFIAvailable();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_MakeMediaBootOption([MarshalAsAttribute(UnmanagedType.LPWStr)]String Description, 
                                                           [MarshalAsAttribute(UnmanagedType.LPWStr)] String DiskLetter, 
                                                           [MarshalAsAttribute(UnmanagedType.LPWStr)] String Path);

        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int UEFI_DeleteBootOptionByDescription([MarshalAsAttribute(UnmanagedType.LPWStr)]String Description);
        #endregion

        public static bool LoadUEFILibrary()
        {
            libHandle = Win32Native.LoadLibrary(@"Win32UEFI.dll");
            if (libHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Log.write(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
                return false;
            }
            return true;
        }

        public static void FreeUEFILibrary()
        {
            Win32Native.FreeLibrary(libHandle);
        }
    }
}
