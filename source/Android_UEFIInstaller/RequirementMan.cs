using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    class RequirementsMan
    {
        List<Func<bool>> RequirementsList = new List<Func<bool>>();

        public RequirementsMan()
        {
            RequirementsList = new List<Func<bool>>{ CheckAdminPrivilege,
                                                     CheckOSArch,
                                                     CheckCPUArch,
                                                     LoadUEFILibrary,
                                                     CheckNVRamAccess,
                                                     CheckUEFI,
                                                     CheckSecureBoot
                                                   };
        }

        public Boolean Run()
        {
            foreach (Func<bool> func in RequirementsList)
            {
                if (!func())
                  {
                      return false;
                  }
            }
            return true;
        }

        private bool CheckSecureBoot()
        {
            //
            // SecureBoot Status
            //
            RegistryKey Subkey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            if (Subkey != null)
            {
                int val = (int)Subkey.GetValue("UEFISecureBootEnabled");
                if (val == 0)
                {
                    Log.write("Secure Boot ... Disabled");
                }
                else
                {
                    Log.write("Secure Boot ... Enabled");
                    return false;
                }
            }
            else
            {
                Log.write("Secure Boot ... Not Supported");
            }

            return true;
        }

        private bool CheckUEFI()
        {
            //
            //UEFI Get
            //
            if (UEFIWrapper.UEFI_isUEFIAvailable())
                Log.write("System Firmware: UEFI");
            else
            {
                Log.write("System Firmware: Other");
                return false;
            }
            return true;
        }

        private bool CheckNVRamAccess()
        {
            WindowsSecurity ws = new WindowsSecurity();
            //
            //NVRAM Access
            //            
            if (ws.GetAccesstoNVRam())
            {
                Log.write("Windows Security: Access NVRAM Privilege ... ok");
            }
            else
            {
                Log.write("Windows Security: Access NVRAM Privilege ... Not All Set");
            }

            return true;
        }

        private bool LoadUEFILibrary()
        {
            return (UEFIWrapper.LoadUEFILibrary());
        }

        private bool CheckCPUArch()
        {
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();

            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("CPU Architecture: " + mo["Architecture"].ToString());
                Log.write("CPU Name: " + mo["Name"].ToString());

                UInt16 Arch = UInt16.Parse(mo["Architecture"].ToString());
                if (Arch == 9) //x64
                {
                    //return true;
                }
            }
            return true;
        }

        private bool CheckOSArch()
        {
            //
            // 64-bit check
            //
            if (!Environment.Is64BitOperatingSystem)
            {
                Log.write("OS Type: 32-bit!");
            }
            //
            // OS Version Get
            //
            Log.write("OSVer: " + Environment.OSVersion.ToString());
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                switch (System.Environment.OSVersion.Version.Major)
                {
                    case 6:
                        if (System.Environment.OSVersion.Version.Minor >= 2)
                            Log.write("OperatingSystem Version ... 6.2");
                        break;
                    case 10:
                        Log.write("OperatingSystem Version ... 10");
                        break;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool CheckAdminPrivilege()
        {
            if (IsAdministrator())
            {
                Log.write("Administrator privilege ... ok");
            }
            else
            {
                Log.write("Administrator privilege ... fail");
                return false;
            }
            return true;
        }

        private bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
