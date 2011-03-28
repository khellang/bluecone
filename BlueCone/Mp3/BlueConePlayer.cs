using System;
using System.IO;
using System.Threading;
using BlueCone.Bluetooth;
using BlueCone.Utils;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.USBHost;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Mp3
{
    /// <summary>
    /// This class is the main BlueCone music player.
    /// </summary>
    public static class BlueConePlayer
    {
        #region Fields

        private static Thread playThread;
        private static AutoResetEvent waitHandle;
        private static Playlist playlist;

        private static PersistentStorage ps;
        private static VolumeInfo volInfo;

        private static string currentTrackPath;
        private static bool cancelPlayback;

        #endregion

        #region Properties

        public static bool CancelPlayback
        {
            get { return cancelPlayback; }
            set { cancelPlayback = value; }
        }

        public static string CurrentTrackPath
        {
            get
            {
                return currentTrackPath;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Method for initializing the musicplayer. Must be called before ANY other method.
        /// </summary>
        public static void Initialize()
        {
            cancelPlayback = false;
            playThread = new Thread(PlayMusic);
            playThread.Priority = ThreadPriority.Highest;
            playlist = new Playlist();
            waitHandle = new AutoResetEvent(false);
            playThread.Start();

            // USB Hosting
            USBHostController.DeviceDisconnectedEvent += new USBH_DeviceConnectionEventHandler(DeviceDisconnected);
            USBHostController.DeviceConnectedEvent += new USBH_DeviceConnectionEventHandler(DeviceConnected);
            RemovableMedia.Eject += new EjectEventHandler(RemovableMedia_Eject);
            RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);

            Debug.Print("BlueConePlayer: Initialized.");
        }

        /// <summary>
        /// This method jumps to the next song in the playlist.
        /// </summary>
        public static void Next()
        {
            cancelPlayback = true;
        }

        /// <summary>
        /// This method jumps back to the previous song in the playlist.
        /// </summary>
        public static void Previous()
        {

        }

        /// <summary>
        /// This method adds a track to the playlist.
        /// </summary>
        /// <param name="path">The path to the song.</param>
        /// <param name="link">The link that added the song.</param>
        public static void AddTrack(string path, Link link)
        {
            int pos = playlist.Enqueue(path, link);
            waitHandle.Set();
            Debug.Print("Received track \"" + path + "\" from " + link);
            WT32.BroadcastMessage("QUEUE#" + pos + "|" + path);
        }

        public static int RemoveTrack(string path)
        {
            return playlist.Remove(path);
        }


        public static void UsePriority()
        {

            playlist.UsePriority();
        }
        /// <summary>
        /// This method sends all the tracks to the specified connection.
        /// </summary>
        /// <param name="connection">The connection to send the tracks to.</param>
        public static void SendTracks(Connection connection)
        {
            if (volInfo != null)
            {
                int totalFiles = DirectoryEx.GetTotalFiles(volInfo.RootDirectory);

                Debug.Print("BlueConePlayer: Sending " + totalFiles + " tracks to link " + connection.Link);
                connection.SendMessage("LISTSTART#" + totalFiles);
                FileStream fs = new FileStream("\\SD\\fileinfo.txt", FileMode.Open, FileAccess.Read, FileShare.None, 512);
                StreamReader sr = new StreamReader(fs);
                string file;
                //while ((file = sr.ReadLine()) != null)
                while ((file = ReadLineEx(sr)) != null)
                {
                    connection.SendMessage("LIST#" + file);
                }

                sr.Close();

                if (currentTrackPath != null)
                    connection.SendMessage("PLAYING#" + currentTrackPath);

                if (playlist.Count > 0)
                {
                    Debug.Print("BlueConePlayer: Sending queue to link " + connection.Link);
                    string[] playQueue = playlist.GetPlaylist();
                    for (int pos = 0; pos < playQueue.Length; pos++)
                        connection.SendMessage("QUEUE#" + pos + "|" + playQueue[pos]);
                }
            }
        }

        public static string ReadLineEx(StreamReader sr)
        {
            int newChar = 0;
            int bufLen = 512; // NOTE: the smaller buffer size.
            char[] readLineBuff = new char[bufLen];
            int growSize = 512;
            int curPos = 0;
            while ((newChar = sr.Read()) != -1)
            {
                if (curPos == bufLen)
                {
                    if ((bufLen + growSize) > 0xffff)
                    {
                        throw new Exception();
                    }
                    char[] tempBuf = new char[bufLen + growSize];
                    Array.Copy(readLineBuff, 0, tempBuf, 0, bufLen);
                    readLineBuff = tempBuf;
                    bufLen += growSize;
                }
                readLineBuff[curPos] = (char)newChar;
                if (readLineBuff[curPos] == '\n')
                {
                    return new string(readLineBuff, 0, curPos);
                }
                if (readLineBuff[curPos] == '\r')
                {
                    if (sr.Peek() == 10)
                    {
                        sr.Read();
                    }
                    return new string(readLineBuff, 0, curPos);
                }
                curPos++;
            }

            if (curPos == 0) return null; // Null fix.
            return new string(readLineBuff, 0, curPos);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// The main thread method that plays the music.
        /// </summary>
        private static void PlayMusic()
        {
            byte[] buffer = new byte[2048];
            int size;
            FileStream file;
            while (true)
            {
                if (playlist.Count <= 0)
                    waitHandle.WaitOne();
                waitHandle.Reset();
                // Get next song from queue
                string song = playlist.Dequeue();
                currentTrackPath = song;
                // Notify connected phones of new song
                WT32.BroadcastMessage("REMOVE#0");
                WT32.BroadcastMessage("PLAYING#" + song);
                Debug.Print("BlueConePlayer: Playing \"" + song + "\"");
                LED.State = LEDState.Playing;
                file = File.OpenRead(song);
                bool DPSSent = false;
                do
                {
                    if (cancelPlayback)
                    {
                        VS1053.CancelPlayback();
                        size = file.Read(buffer, 0, buffer.Length);
                        VS1053.SendData(buffer);
                        break;
                    }
                    else
                    {
                        size = file.Read(buffer, 0, buffer.Length);
                        VS1053.SendData(buffer);
                        int decodeTime = VS1053.GetDecodeTime();
                        if (decodeTime >= 10 && !DPSSent)
                        {
                            int byteRate = VS1053.GetByteRate();
                            int playPercent = (int)((double)((double)(decodeTime * byteRate) / file.Length) * 100);
                            if (playPercent > 1)
                            {
                                WT32.BroadcastMessage("DECODE#" + decodeTime + "|" + playPercent);
                                DPSSent = true;
                            }
                        }
                    }
                } while (size > 0);
                // Song finished playing
                VS1053.Reset();
                currentTrackPath = null;
                file.Close();
                file.Dispose();
                LED.State = LEDState.Ready;
            }
        }

        /// <summary>
        /// This method is called when a device is plugged into the USB host.
        /// </summary>
        /// <param name="device">The device detected.</param>
        private static void DeviceConnected(USBH_Device device)
        {
            switch (device.TYPE)
            {
                case USBH_DeviceType.MassStorage:
                    ps = new PersistentStorage(device);
                    ps.MountFileSystem();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// This method is called when a device is pulled out of the USB host.
        /// </summary>
        /// <param name="device">The device pulled out.</param>
        private static void DeviceDisconnected(USBH_Device device)
        {
            ps.UnmountFileSystem();
            ps.Dispose();
            ps = null;
        }

        /// <summary>
        /// This method is called when the USB storage device is mounted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private static void RemovableMedia_Insert(object sender, MediaEventArgs e)
        {

            volInfo = e.Volume;
            Mp3Info.SaveInfo();
            foreach (Connection connection in WT32.Connections.Values)
            {
                SendTracks(connection);
            }
        }

        /// <summary>
        /// This method is called when the USB storage device is unmounted.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private static void RemovableMedia_Eject(object sender, MediaEventArgs e)
        {
            volInfo = null;
        }

        #endregion

        #endregion
    }
}
