using System;
using System.IO;

namespace Android_UEFIInstaller
{
    class BootloaderUEFIInstaller
    {
        public bool InstallBootObjects(Object extraData)
        {
            String EFI_DIR = config.UEFI_PARTITION_MOUNTPOINT + config.UEFI_DIR;
            Log.write("[Installing Boot Objects]");

            if (!Utils.MountFirmwarePartition())
                return false;

            if (!CreateBootDirectory(EFI_DIR))
                return false;

            if (!CopyBootFiles(EFI_DIR))
                return false;


            if (!CreateUEFIBootOption(config.UEFI_PARTITION_MOUNTPOINT))
                return false;


            if (!Utils.UnMountFirmwarePartition())
                return false;

            return true;
        }

        public bool UnInstallBootObjects(Object extraData)
        {

            Log.write("[Removing Boot Objects]");
            Utils.MountFirmwarePartition();
            if (UEFIWrapper.UEFI_Init())
            {
                Log.write("    -Remove Android UEFI Entry");
                int ret = UEFIWrapper.UEFI_DeleteBootOptionByDescription(config.BOOT_ENTRY_TEXT);
                Log.write("    -UEFI: " + ret);
            }
            else
            {
                Log.write("    -UEFI Init ... fail");
            }
            Utils.CleanupDirectory(config.UEFI_PARTITION_MOUNTPOINT + config.UEFI_DIR);
            Utils.UnMountFirmwarePartition();

            return true;
        }

        private Boolean CreateBootDirectory(string directory)
        {
            
            Log.write("  Setup Boot Directory...");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Log.write("    Boot Folder Created: " + directory);
            }
            else
            {
                Log.write("    Boot Directory is Already Exist");
                return false;
            }

            return true;
        }

        private Boolean CopyBootFiles(String directory)
        {
            Log.write("  Copy Boot files");
            try
            {
                if (Environment.Is64BitOperatingSystem)
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_BIN64, directory + @"\" + config.UEFI_GRUB_BIN64, false);
                else
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_BIN32, directory + @"\" + config.UEFI_GRUB_BIN32, false);
                
                File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_CONFIG, directory + @"\" + config.UEFI_GRUB_CONFIG, false);
                return true;
            }
            catch (Exception ex)
            {
                Log.write(ex.Message);
                return false;
            }
            
        }

        private Boolean CreateUEFIBootOption(String drive)
        {
            String _Drive = String.Format(@"\\.\{0}",drive);

            Log.write("  Add UEFI Entry");
            
            if (!UEFIWrapper.UEFI_Init())
            {
                Log.write("    UEFI Init Fail");
                return false;
            }

            if (Environment.Is64BitOperatingSystem)
            {

                if (!UEFIWrapper.UEFI_MakeMediaBootOption(config.BOOT_ENTRY_TEXT, _Drive, config.UEFI_DIR + config.UEFI_GRUB_BIN64))
                {
                    Log.write("    UEFI 64-bit Entry Fail");
                    return false;
                }
            }
            else
            {
                if (!UEFIWrapper.UEFI_MakeMediaBootOption(config.BOOT_ENTRY_TEXT, _Drive, config.UEFI_DIR + config.UEFI_GRUB_BIN32))
                {
                    Log.write("    UEFI 32-bit Entry Fail");
                    return false;
                }
            }
            return true;
        }
    }
}
