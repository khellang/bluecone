using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using System.IO;
using System.Xml;
using BlueCone.Bluetooth;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public static class Settings
    {
        #region Fields

        private static PersistentStorage storage;
        private static VolumeInfo SDVolInfo;

        private static bool usePriority = true;
        private static string masterPassword = "blue123";
        private static string pairingKey = "1234";
        private static bool playRandom = true;
        private static double volume = 1;

        #endregion

        #region Properties

        public static bool UsePriority
        {
            get
            {
                return usePriority;
            }
            set
            {
                usePriority = value;
                Save();
            }
        }

        public static string MasterPassword
        {
            get
            {
                return masterPassword;
            }
            set
            {
                masterPassword = value;
                Save();
            }
        }

        public static string PairingKey
        {
            get
            {
                return pairingKey;
            }
            set
            {
                pairingKey = value;
                Save();
            }
        }

        public static bool PlayRandom
        {
            get
            {
                return playRandom;
            }
            set
            {
                playRandom = value;
                Save();
            }
        }

        public static double Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                Save();
            }
        }

        #endregion

        #region Methods

        public static void Load()
        {
            if (PersistentStorage.DetectSDCard())
            {
                storage = new PersistentStorage("SD");
                storage.MountFileSystem();
                RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);
            }  
        }

        static void RemovableMedia_Insert(object sender, MediaEventArgs e)
        {
            if (e.Volume.RootDirectory == @"\SD")
            {
                string path = @"\SD\settings.xml";
                SDVolInfo = e.Volume;
                if (File.Exists(path))
                {
                    Debug.Print("BlueCone: Settings found. Loading...");
                    using (FileStream settingStream = File.OpenRead(path))
                    using (XmlReader reader = XmlReader.Create(settingStream))
                        while (reader.Read())
                            if (reader.IsStartElement())
                            switch (reader.Name)
                            {
                                case "usePriority":
                                    usePriority = ConvertString.ToBoolean(reader.ReadString());
                                    break;
                                case "masterPassword":
                                    masterPassword = reader.ReadString();
                                    break;
                                case "pairingKey":
                                    pairingKey = reader.ReadString();
                                    break;
                                case "playRandom":
                                    playRandom = ConvertString.ToBoolean(reader.ReadString());
                                    break;
                                case "volume":
                                    volume = double.Parse(reader.ReadString());
                                    break;
                                default:
                                    break;
                            }
                    Debug.Print("BlueCone: Settings loaded.");
                }
                else
                {
                    Debug.Print("BlueCone: Could not find settings. Using default.");
                    Save();
                }
            }
        }

        public static void Save()
        {

        }


        #endregion
    }
}
