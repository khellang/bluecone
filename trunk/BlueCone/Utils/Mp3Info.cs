using System;
using Microsoft.SPOT;
using System.Collections;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.USBHost;
using System.Xml;
using System.Ext.Xml;
using System.IO;
using BlueCone.Utils.ID3;


namespace BlueCone.Utils
{
    public class Mp3Info
    {

        #region Fields

        private static VolumeInfo SDVolInfo = Settings.SDVolume;
      
        #endregion

        public static void SaveInfo()
        {
            string filename = "fileinfo.txt";

            Debug.Print("Writing to ID3 info to " + filename);

            if (File.Exists(@"\SD\" + filename))
                File.Delete(@"\SD\" + filename);

            FileStream fs = new FileStream(@"\SD\" + filename, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string track in DirectoryEx.GetFiles(@"\USB\"))
            {
                ID3Tag temp = ID3TagReader.ReadFile(track);
                sw.WriteLine(temp.Path + "|" + temp.Artist + "|" + temp.Album + "|" + temp.Title);
            }

            sw.Close();
           
            Debug.Print("Finished writing to file");
        }
        
    }
}
