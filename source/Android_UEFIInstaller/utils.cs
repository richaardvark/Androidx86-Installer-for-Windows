using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    public static class Utils
    {
        public static String SearchForPreviousInstallation(String FolderName)
        {
            String[] drives = Environment.GetLogicalDrives();

            foreach (String drive in drives)
            {
                if (Directory.Exists(drive + FolderName))
                {
                    return drive.Substring(0, 1);
                }
            }

            return "0";
        }

        public static Boolean SetupDirectories(String[] directoryList)
        {
            Log.write("    -Setup Directories...");
            foreach (String directory in directoryList)
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Log.write("    -Directory Created: " + directory);
                        return true;
                    }
                    else
                    {

                        Log.write(directory + " Already Exists");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.write("Error Creating OS directory:" + ex.Message.ToString() + "Dir:" + directory);
                    return false;
                }
            }
            return false;
        }

        public static Boolean ExtractArchive(String srcArchive, String ExtractDirectory, List<String> fileNames)
        {
            //7z.exe x android-x86-4.4-r2.img "efi" "kernel" "ramdisk.img" "initrd.img" "system.sfs" -o"C:\Users\ExtremeGTX\Desktop\installer_test\extracted\"
            string strFileNames = "\"" + String.Join("\" \"", fileNames.ToArray()) + "\"";
            string ExecutablePath = Environment.CurrentDirectory + @"\7z.exe";
            string ExecutableArgs = String.Format(" x \"{0}\" {1} -o{2}", srcArchive, strFileNames, ExtractDirectory );    //{0} ISO Filename, {1} extraction dir
            
            //
            //Extracting Archive Contents
            //
            Log.write("    -Extracting Archive...");
            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;
           
            return true;
        }

        public static Boolean ExtractSFS(String srcArchive, String ExtractDirectory)
        {
            //7z.exe x android-x86-4.4-r2.img "efi" "kernel" "ramdisk.img" "initrd.img" "system.sfs" -o"C:\Users\ExtremeGTX\Desktop\installer_test\extracted\"
            string ExecutablePath = Environment.CurrentDirectory + @"\unsquashfs.exe";
            string ExecutableArgs = String.Format(" -f -d \"{0}\" \"{1}\"", ExtractDirectory, srcArchive);    //{0} ISO Filename, {1} extraction dir

            //
            //Extracting Archive Contents
            //
            Log.write("    -Extracting SquashFS Image...");
            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;
        }

        public static Boolean VerifyFiles(List<String> FileList, String directory=null)
        {
            String filePath;
            foreach (String file in FileList)
            {
                filePath = directory + "\\" + file;
                if (!File.Exists(filePath))
                {
                    Log.write("File: " + filePath + " not exist");
                    return false;
                }
            }
            Log.write("    -Verification completed successfully");
            return true;
        }

        public static Boolean CreateDataParition(String directory, String Size)
        {

            Log.write("    -Creating Data.img");

            string ExecutablePath = Environment.CurrentDirectory + @"\dd.exe";
            string ExecutableArgs = String.Format(@"if=/dev/zero of={0}\data.img count={1}", directory, Size.ToString());

            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;

        }

        public static Boolean FormatDataPartition(String FilePath)
        {
            Log.write("    -Initializing Data.img");
            string ExecutablePath = Environment.CurrentDirectory + @"\mke2fs.exe";
            string ExecutableArgs = String.Format("-F -t ext4 \"{0}\\data.img\"", FilePath);

            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;
        }

        public static Boolean CleanupDirectory(String directory)
        {
            Log.write("    -Cleaning up Android Directory ... " + directory);
            try
            {
                //Get if Directory Exist
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Exception: " + ex.Message);
                return false;
            }

        }

        public static Boolean ExecuteCLICommand(String FilePath, String args)
        {
            string CliExecutable = FilePath;
            string CliArguments = args;
            try
            {

                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                Log.write("    #Launch:" + CliExecutable + CliArguments);
                p.StartInfo.FileName = CliExecutable;
                p.StartInfo.Arguments = CliArguments;
                p.Start();
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    Log.write(String.Format("Error Executing {0} with Args: {1}", FilePath, args));
                    Log.write("Error output:");
                    Log.write(p.StandardError.ReadToEnd());
                    Log.write(p.StandardOutput.ReadToEnd());
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Exception: " + ex.Message);
                return false;
            }
        }

        public static Boolean MountFirmwarePartition()
        {
            Log.write("    -Mounting EFI Partition...");

            string MOUNT_EXE = @"C:\Windows\System32\mountvol.exe";
            string MOUNT_CMD = String.Format(" Z: /S");


            if (!Utils.ExecuteCLICommand(MOUNT_EXE, MOUNT_CMD))
            {
                return false;
            }

            return true;
        }

        public static Boolean UnMountFirmwarePartition()
        {
            Log.write("    -UnMounting EFI Partition...");
            string UNMOUNT_EXE = @"C:\Windows\System32\mountvol.exe";
            string UNMOUNT_CMD = String.Format(" Z: /D");

            if (!Utils.ExecuteCLICommand(UNMOUNT_EXE, UNMOUNT_CMD))
            {
                return false;
            }

            return true;
        }
        
        public static long GetTotalFreeSpace(string driveName)
        {
            driveName = String.Format("{0}:\\", driveName.Substring(0, 1));

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }

        public static String GetDriveType(string driveName)
        {
            driveName = String.Format("{0}:\\", driveName.Substring(0, 1));
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.DriveType.ToString();
                }
            }
            return "ERROR";
        }

        public static String GetDriveFormat(string driveName)
        {
            driveName = String.Format("{0}:\\", driveName.Substring(0, 1));
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.DriveFormat;
                }
            }
            return "ERROR";
        }

        public static String GetSizeInBytes(double SizeInGB)
        {
            return Convert.ToUInt64((SizeInGB * 1024 * 1024 * 1024) / 512).ToString();
        }

        public static bool IsBitlockerEnabled(String driveName)
        {
            ManagementPath path = new ManagementPath(@"\ROOT\CIMV2\Security\MicrosoftVolumeEncryption") { ClassName = "Win32_EncryptableVolume" };
            ManagementScope scope = new ManagementScope(path);
            path.Server = Environment.MachineName;
            System.Management.ManagementClass objectSearcher = new ManagementClass(scope, path, new ObjectGetOptions());
            
            foreach (var item in objectSearcher.GetInstances())
            {
                try
                {
                    if (item["DriveLetter"].ToString() == driveName)
                    {
                        if (item["ProtectionStatus"].ToString() == "0")
                            return false;
                        else
                            return true;
                    }
                }
                catch(Exception ex)
                {
                    Log.write("Exception: " + ex.Message);
                    return true;
                }
            }

            Log.write("Can't get Bitlocker status for drive " + driveName);
            return false;
        }
        
    }
}
