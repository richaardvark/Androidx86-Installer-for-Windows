using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android_UEFIInstaller.variants.android_x86;
using System.IO;

namespace Android_UEFIInstaller
{
    

    public class AndroidInstaller
    {
        PrivilegeClass.Privilege FirmwarePrivilege;
        

        public AndroidInstaller(String ISOFilePath, String InstallDrive, String UserDataSize)
        {
            FirmwarePrivilege = new PrivilegeClass.Privilege("SeSystemEnvironmentPrivilege");

            InstallationConfig.InstallDirectory = String.Format(config.INSTALL_DIR, InstallDrive);
            InstallationConfig.ISOFilePath = ISOFilePath;
            InstallationConfig.InstallDrive = InstallDrive;
            InstallationConfig.UserDataSize = UserDataSize;
            if (ISOFilePath != null)
            {
                if (!InstallationConfig.ISOFilePath.Contains("PhoenixOS"))
                {
                    InstallationConfig.fileList = new List<string>()
                    {
                        "kernel",
                        "ramdisk.img",
                        "initrd.img",
                        "system.sfs"
                    };
                }
                else
                {
                    InstallationConfig.fileList = new List<string>()
                    {
                        "kernel",
                        "ramdisk.img",
                        "initrd.img",
                        "system.img"
                    };
                }
            }

        }
        
        public Boolean Run(InstallationOperation SysOperation)
        {
            Boolean ret;
            FirmwarePrivilege.Enable();

            switch (SysOperation)
            {
                case InstallationOperation.SYS_INSTALL:
                    ret = InstallAndroid();
                    break;

                case InstallationOperation.SYS_REMOVE:
                    ret = UninstallAndroid();
                    break;

                case InstallationOperation.SYS_UPDATE:
                    
                    ret = UpdateAndroid();
                    break;
                
                default:
                    ret = false;
                    break;
            }
            FirmwarePrivilege.Revert();
            return ret;
        }

        private Boolean InstallAndroid()
        {
            List<IInstallStep> ActionsList = new List<IInstallStep>();
            ActionsList.Add(new variants.android_x86.SetupDirectories());
            ActionsList.Add(new variants.android_x86.ExtractFiles());
            if (!InstallationConfig.ISOFilePath.Contains("PhoenixOS"))
            {
                ActionsList.Add(new variants.android_x86.ExtractSFS());
            }
            ActionsList.Add(new variants.android_x86.Verify());
            ActionsList.Add(new variants.android_x86.CreateDataPartition());
            ActionsList.Add(new variants.android_x86.FormatDataPartition());
            ActionsList.Add(new variants.android_x86.InstallBootObjects());

            LogInstallationInfo();
            Log.write("[Installing OS Image]");

            foreach (IInstallStep s in ActionsList)
            {
                Log.updateStatus(s.GetDescription());
                Log.write("  " + s.GetDescription());
                if(!s.Run())
                {
                    s.Revert();
                    new variants.android_x86.CleanUpAll().Run();
                    return false;
                }
            }
            Log.updateStatus("Installing Android Finished");
            return true;
        }

        private Boolean UpdateAndroid()
        {
            List<IInstallStep> ActionsList = new List<IInstallStep>();
            ActionsList.Add(new variants.android_x86.CleanUpSystem());
            ActionsList.Add(new variants.android_x86.ExtractFiles());
            if (!InstallationConfig.ISOFilePath.Contains("PhoenixOS"))
            {
                ActionsList.Add(new variants.android_x86.ExtractSFS());
            }
            ActionsList.Add(new variants.android_x86.Verify());

            LogInstallationInfo();
            Log.write("[Updating Current Installation]");

            foreach (IInstallStep s in ActionsList)
            {
                Log.updateStatus(s.GetDescription());
                Log.write(s.GetDescription());

                if (!s.Run())
                {
                    s.Revert();
                    new variants.android_x86.CleanUpAll().Run();
                    return false;
                }
            }

            Log.updateStatus("Updating Android Finished");
            return true;
        }

        //TODO: Needs to be modified to use IInstallStep
        private Boolean UninstallAndroid(String InstallDrive = "0")
        {
            Log.write(String.Format("====Uninstall Started on {0}====", DateTime.Now));

            InstallDrive = Utils.SearchForPreviousInstallation(config.INSTALL_FOLDER);
            if (InstallDrive != "0")
            {
                Utils.CleanupDirectory(String.Format(config.INSTALL_DIR, InstallDrive));
            }
            else
            {
                Log.write("Android Installation Not Found");
            }
            new BootloaderUEFIInstaller().UnInstallBootObjects(null);

            Log.updateStatus("Cleanup Finshed!");
            return true;
        }


        private void LogInstallationInfo()
        {
            Log.write(String.Format("====Installation Started on {0}====", DateTime.Now));
            Log.write("[Installation info]");
            Log.write("  OS Image: " + InstallationConfig.ISOFilePath);
            Log.write("  Target Drive: " + InstallationConfig.InstallDrive);
            Log.write("  Free Space: " + Utils.GetTotalFreeSpace(InstallationConfig.InstallDrive));
            Log.write("  Type: " + Utils.GetDriveType(InstallationConfig.InstallDrive));
            Log.write("  Format: " + Utils.GetDriveFormat(InstallationConfig.InstallDrive));
            Log.write("  UserDataSize: " + InstallationConfig.UserDataSize);
        }
    }
}
