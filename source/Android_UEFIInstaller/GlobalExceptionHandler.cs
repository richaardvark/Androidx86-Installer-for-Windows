using System;


namespace Android_UEFIInstaller
{
    public static class GlobalExceptionHandler
    {
        public static void SetupExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += MyHandler;
            AppDomain.CurrentDomain.ProcessExit += currentDomain_ProcessExit;

        }

        static void currentDomain_ProcessExit(object sender, EventArgs e)
        {
            UEFIWrapper.FreeUEFILibrary();
            Log.save();
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.write("MyHandler caught : " + e.Message);
            Log.write(String.Format("Runtime terminating: {0}", args.IsTerminating));
            Log.save();
        }
    }
}
