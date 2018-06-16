using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller.variants.android_x86
{
    static class InstallationConfig
    {
        /* Installation Config */
        public static String InstallDirectory;
        public static String ISOFilePath;
        public static String InstallDrive;
        public static String UserDataSize;
        public static List<String> fileList;
    }

    class SetupDirectories : IInstallStep
    {
        public String GetDescription() { return "Setting up installation Directory"; }

        public bool Run(object Args = null)
        {
            if(!Utils.SetupDirectories(new String[]{InstallationConfig.InstallDirectory}))
            {
                return false;
            }
            return true;
        }

        public bool Revert(object Args = null)
        {
            throw new NotImplementedException();
        }
    }
    class ExtractFiles : IInstallStep
    {
        public String GetDescription() { return "Extracting OS Image"; }
        public bool Run(object Args)
        {
            if (!Utils.ExtractArchive(InstallationConfig.ISOFilePath,
                                      InstallationConfig.InstallDirectory,
                                      InstallationConfig.fileList)
                   )
            {
                Log.write("Failed to Extract OS Image");
                return false;
            }

            return true;
        }

        public bool Revert(object Args)
        {
            throw new NotImplementedException();
        }
    }
    class ExtractSFS : IInstallStep
    {
        public String GetDescription() { return "Extracting SFS Image"; }
        public bool Run(object Args)
        {
            if (!Utils.ExtractSFS(InstallationConfig.InstallDirectory + @"\system.sfs",
                                      InstallationConfig.InstallDirectory)
                   )
            {
                Log.write("Failed to Extract SFS Image");
                return false;
            }

            return true;
        }

        public bool Revert(object Args)
        {
            throw new NotImplementedException();
        }
    }
    class Verify : IInstallStep
    {
        public String GetDescription() { return "Verifying"; }

        public bool Run(Object Args = null)
        {
            if(!Utils.VerifyFiles(InstallationConfig.fileList, InstallationConfig.InstallDirectory))
            {
                Log.write("Verification error");
                return false;
            }

            return true;
        }
        
        public bool Revert(Object Args = null)
        {
            throw new NotImplementedException();
        }

    }
    class CreateDataPartition : IInstallStep
    {
        public String GetDescription() { return "Creating Data.img"; }

        public bool Run(object Args = null)
        {
            if(!Utils.CreateDataParition(InstallationConfig.InstallDirectory, InstallationConfig.UserDataSize))
            {
                Log.write("Failed to creating Data.img");
                return false;
            }
            return true;
        }

        public bool Revert(object Args = null)
        {
            throw new NotImplementedException();
        }
    }
    class FormatDataPartition : IInstallStep
    {
        public String GetDescription() { return "formatting Data.img"; }

        public bool Run(object Args = null)
        {
            if (!Utils.FormatDataPartition(InstallationConfig.InstallDirectory))
            {
                Log.write("Failed to formating Data.img");
                return false;
            }
            return true;
        }

        public bool Revert(object Args = null)
        {
            throw new NotImplementedException();
        }
    }

    class InstallBootObjects : IInstallStep
    {
        public String GetDescription() { return ""; }

        public bool Run(object Args = null)
        {
            BootloaderUEFIInstaller installer = new BootloaderUEFIInstaller();

            if (!installer.InstallBootObjects(null))
            {
                Log.write("Failed to install Boot objects");
                return false;
            }
            return true;
        }

        public bool Revert(object Args = null)
        {
            throw new NotImplementedException();
        }
    }

    class CleanUpAll : IInstallStep
    {
        public String GetDescription() { return "Cleaning up Android (Full)"; }
        public bool Run(object Args = null)
        {
            
            String InstallDrive = Utils.SearchForPreviousInstallation(config.INSTALL_FOLDER);
            if (InstallDrive != "0")
            {
                Utils.CleanupDirectory(String.Format(config.INSTALL_DIR, InstallDrive));
            }
            else
            {
                Log.write("Android Installation Not Found");
            }

            Log.write("Cleaning up Android (Full) @ " + InstallationConfig.InstallDirectory);
            new BootloaderUEFIInstaller().UnInstallBootObjects(null);
            Log.updateStatus("Cleanup complete!");

            return true;
        }

        public bool Revert(object Args = null)
        {
            return true;
        }
    }

    class CleanUpSystem : IInstallStep
    {
        public String GetDescription() { return "Cleaning up Android (System ONLY)"; }

        public bool Run(object Args = null)
        {
            List<String> fileList = new List<string>() { "kernel", 
                                                         "ramdisk.img", 
                                                         "initrd.img", 
                                                         "system.sfs",
                                                         "system.img"
                                                       };
            
            String InstallDrive = Utils.SearchForPreviousInstallation(config.INSTALL_FOLDER);
            if (InstallDrive == "0")
            {
                Log.write("Android Installation Not Found");
            }
            InstallationConfig.InstallDirectory = String.Format(config.INSTALL_DIR, InstallDrive);
            Log.write("Cleaning up Android @ " + InstallationConfig.InstallDirectory);
            try
            {
                //Get if Directory Exist
                if (Directory.Exists(InstallationConfig.InstallDirectory))
                {
                    foreach (string f in fileList)
                    {
                        if (File.Exists(InstallationConfig.InstallDirectory + "\\" + f))
                        {
                            File.Delete(InstallationConfig.InstallDirectory + "\\" + f);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Exception: " + ex.Message);
                return false;
            }
        }

        public bool Revert(object Args = null)
        {
            throw new NotImplementedException();
        }
    }
}
