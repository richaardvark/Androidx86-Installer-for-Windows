using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    class MachineSpecs
    {
        public MachineSpecs()
        {

        }
        
        public void Get()
        {
            //
            // Machine Info
            //
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();

            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("Manufacturer: " + mo["Manufacturer"].ToString());
                Log.write("Model: " + mo["Model"].ToString());
            }

            //
            // Motherboard Model
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_BaseBoard");
            osDetailsCollection = objOSDetails.Get();
            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("Product: " + mo["Product"].ToString());
            }

            //
            // BIOS Version
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_BIOS");
            osDetailsCollection = objOSDetails.Get();
            foreach (ManagementObject mo in osDetailsCollection)
            {
                String[] iBIOS = (String[])mo["BIOSVersion"];
                Log.write("BIOS info:");
                foreach (String item in iBIOS)
                {
                    Log.write(item);
                }
            }

            //
            // Graphics Card type
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_VideoController");
            osDetailsCollection = objOSDetails.Get();
            Log.write("Available GPU(s):");
            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("GPU: " + mo["Description"].ToString());
            }
        }
    }
}
