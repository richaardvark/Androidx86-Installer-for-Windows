using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Android_UEFIInstaller
{
    /// <summary>
    /// Interaction logic for AndroidMaterial.xaml
    /// </summary>
    public partial class AndroidMaterial : Window
    {
     
        BackgroundWorker InstallationTask;
        long OSSize = 0;
        bool Update = false;

        public AndroidMaterial()
        {
            InitializeComponent();
            
            Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Title += String.Format("v{0}.{1}",v.Major.ToString(), v.Minor.ToString());
            //
            //Update Version
            //
            txtVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //
            //Setup TxtLog for logging
            //
            Log.SetLogBuffer(txtlog);
            Log.SetStatuslabel(lblStatus);
            //
            //SetupGlobalExceptionHandler
            //
            GlobalExceptionHandler.SetupExceptionHandler();
            //
            //Log Some Info
            //
            Log.write("================Installer Info================");
            Log.write("Installer Directory:" + Environment.CurrentDirectory);
            Log.write("Installer Version:" + System.Reflection.Assembly.GetExecutingAssembly()
                                            .GetName()
                                            .Version
                                            .ToString());
            //
            // Machine Info
            //
            new MachineSpecs().Get();
           
            //
            // Check if Requirements satisifed
            //
            if (!new RequirementsMan().Run())
            {
                UpdateUI(UI_STATUS.OFF);
                MessageBox.Show("Not all system requirements are met\nPlease check the installer log");
            }
           
            //
            // Initialize Installation Task
            //
            InitInstallationTask();
           
            //
            // Check for previous installation
            //
            CheckForAndroidInstallation();

            Log.write("==========================================");
        }

        
        

        private void Btn_Install(object sender, RoutedEventArgs e)
        {
            TaskInfo InstallInfo;
            String Path=txtISOPath.Text;
            String Drive=cboDrives.Text.Substring(0, 1);
            String Size = Utils.GetSizeInBytes(sldrSize.Value);


            UpdateUI(UI_STATUS.DISABLE);

            if (Update)
            {
                InstallInfo = new TaskInfo
                {
                    path = Path,
                    drive = Drive,
                    size = Size,
                    operation = InstallationOperation.SYS_UPDATE
                };
            }
            else
            {
                InstallInfo = new TaskInfo
                {
                    path = Path,
                    drive = Drive,
                    size = Size,
                    operation = InstallationOperation.SYS_INSTALL
                };
            }

            InstallationTask.RunWorkerAsync(InstallInfo);            
        }

        private void Btn_Uninstall(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.No == MessageBox.Show("Are you sure you want to remove android ?", "Android Installer", MessageBoxButton.YesNo, MessageBoxImage.Question))
                return;

            UpdateUI(UI_STATUS.DISABLE);

            TaskInfo InstallInfo = new TaskInfo
            {
                path = null,
                drive = null,
                size = null,
                operation = InstallationOperation.SYS_REMOVE
            };

            InstallationTask.RunWorkerAsync(InstallInfo);
            
        }


        private void sldrSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldrSize.IsLoaded)
            {
                CheckUserSettings();
            }
        }
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (String item in Environment.GetLogicalDrives())
            {
                /* Remove trailing slash */
                cboDrives.Items.Add(item.Substring(0,2));
                
            }
            cboDrives.SelectedIndex = 0;
        }

        

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".img";
            dlg.Filter = "Android System Image |*.iso;*.img;*.zip";

            if (dlg.ShowDialog() == true)
            {
                txtISOPath.Text = dlg.FileName;
                OSSize = new FileInfo(dlg.FileName).Length;
                UpdateAvailableDiskSpace();
                CheckUserSettings();
            }
        }

        private void cboDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            UpdateAvailableDiskSpace();
            CheckUserSettings();
        }

        private void txtlog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtlog.ScrollToEnd();
        }


        #region "InstallationTask"
        
        void InitInstallationTask()
        {
            InstallationTask = new BackgroundWorker();
            InstallationTask.WorkerReportsProgress = false;
            InstallationTask.DoWork += InstallationTask_DoWork;
            InstallationTask.ProgressChanged += InstallationTask_ProgressChanged;
            InstallationTask.RunWorkerCompleted += InstallationTask_RunWorkerCompleted;
        }

        void InstallationTask_DoWork(object sender, DoWorkEventArgs e)
        {
            TaskInfo InstallInfo = (TaskInfo)e.Argument;
            AndroidInstaller installationEngine = new AndroidInstaller(InstallInfo.path, 
                                                                           InstallInfo.drive, 
                                                                           InstallInfo.size
                                                                           );


            if (!installationEngine.Run(InstallInfo.operation))
                MessageBox.Show("Operation Failed" + Environment.NewLine + "Please check log at C:\\AndroidInstall_XXX.log");
            else
                MessageBox.Show("Operation Completed successfully");


            
        }

        void InstallationTask_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void InstallationTask_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateUI(UI_STATUS.ENABLE);
        }

        #endregion



        void CheckUserSettings()
        {
            String Path = txtISOPath.Text;
            String Drive = cboDrives.SelectedItem.ToString();
            String Size = Utils.GetSizeInBytes(sldrSize.Value);

            if (!File.Exists(Path))
            {
                cmdInstall.IsEnabled = false;
                return;
            }

            if (!Update) 
            {
                if (Size == "0") //Size shouldn't be Zero for fresh install
                {
                    cmdInstall.IsEnabled = false;
                    return;
                }
            }

            if (Utils.IsBitlockerEnabled(Drive))
            {
                cmdInstall.IsEnabled = false;
                return;
            }

            cmdInstall.IsEnabled = true;
        }

        void CheckForAndroidInstallation()
        {
            if (Utils.SearchForPreviousInstallation("AndroidOS") != "0")
            {
                Log.write("Previous Android installation found!");
                Update = true;
                cmdInstall.Content = "Update";
            }
        }

        void UpdateAvailableDiskSpace()
        {
            long DiskSize = Utils.GetTotalFreeSpace(cboDrives.SelectedItem.ToString());

            sldrSize.Maximum = ((DiskSize - OSSize + 524288000) / 1024 / 1024 / 1024);
            sldrSize.Value = 0.1 * sldrSize.Maximum;
            sldrSize.TickFrequency = 0.01 * sldrSize.Maximum;
        }

        void UpdateUI(UI_STATUS status)
        {

            switch (status)
            {
                case UI_STATUS.ENABLE:
                    cmdInstall.IsEnabled = true;
                    cmdRemove.IsEnabled = true;
                    cboDrives.IsEnabled = true;
                    sldrSize.IsEnabled = true;
                    ImgCmdBrowse.IsEnabled = true;
                    pbarStatus.IsIndeterminate = false;
                    break;

                case UI_STATUS.DISABLE:
                    pbarStatus.IsIndeterminate = true;
                    cmdInstall.IsEnabled = false;
                    cmdRemove.IsEnabled = false;
                    cboDrives.IsEnabled = false;
                    sldrSize.IsEnabled = false;
                    ImgCmdBrowse.IsEnabled = false;
                    break;

                case UI_STATUS.OFF:
                    cmdInstall.IsEnabled = false;
                    cmdRemove.IsEnabled = false;
                    cboDrives.IsEnabled = false;
                    sldrSize.IsEnabled = false;
                    ImgCmdBrowse.IsEnabled = false;
                    break;

                case UI_STATUS.READY:
                    //pbarStatus.IsIndeterminate = false;
                    break;
                default:
                    break;
            }
        }

    }
  
}
