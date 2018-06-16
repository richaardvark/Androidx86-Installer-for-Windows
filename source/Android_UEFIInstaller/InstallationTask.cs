using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    class InstallationTask : BackgroundWorker
    {
        protected override void OnDoWork(DoWorkEventArgs e)
        {
            TaskInfo InstallInfo = (TaskInfo)e.Argument;
            AndroidInstaller installationEngine = new AndroidInstaller(InstallInfo.path,
                                                                           InstallInfo.drive,
                                                                           InstallInfo.size
                                                                           );



            if (!installationEngine.Run(InstallInfo.operation))
                Log.write("Operation Failed" + Environment.NewLine + "Please check log at C:\\AndroidInstall_XXX.log");
            else
                Log.write("Operation Completed successfully");
        }

        protected override void OnProgressChanged(ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
