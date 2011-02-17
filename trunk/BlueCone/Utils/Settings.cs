using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.IO;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public class Settings
    {
        #region Fields

        private static PersistentStorage storage;

        private static bool usePriority = true;
        private static string masterPassword = "blue123";
        private static int pairingKey = 1234;
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

        public static int PairingKey
        {
            get
            {
                return pairingKey;
            }
            set
            {
                pairingKey = value;
                Save();
                // TODO: Set pairingkey in bluetooth module...
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

        #region Ctor

        public Settings()
        {

        }

        #endregion

        #region Methods

        public static void Load()
        {
            if (PersistentStorage.DetectSDCard())
            {
                storage = new PersistentStorage("SD");
                storage.MountFileSystem();
            }  
        }

        public static void Save()
        {

        }


        #endregion
    }
}
