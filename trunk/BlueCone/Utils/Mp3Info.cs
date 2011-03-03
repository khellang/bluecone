using System;
using Microsoft.SPOT;
using System.Collections;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.USBHost;
using System.Xml;
using System.Ext.Xml;
using System.IO;


namespace BlueCone.Utils
{
    public class Mp3Info
    {

        #region Fields

        private static PersistentStorage storage;
        private static VolumeInfo SDVolInfo = Settings.SDVolume;

        
        #endregion

        public static void SaveInfo()
        {
            FileStream fs = new FileStream("\\SD\\fileinfo.txt", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string track in DirectoryEx.GetFiles(@"\USB\"))
            {
                string[] temp = ID3TagReader.ReadFile(track);
                sw.WriteLine(temp[0] + "|" + temp[1] + "|" + temp[2] + "|" + temp[3]);
            }
            sw.Flush();
            sw.Close();
            
            
            Debug.Print("Entry made");
        }
        
        
    }
}
