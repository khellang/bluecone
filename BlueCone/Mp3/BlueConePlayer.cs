using System;
using Microsoft.SPOT;
using System.Threading;
using BlueCone.Bluetooth;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using System.IO;
using BlueCone.Utils;
using System.Runtime.CompilerServices;

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

        private static PlaybackStatus status;

        private static Thread playThread;
        private static AutoResetEvent waitHandle;
        private static Playlist playlist;

        private static PersistentStorage ps;
        private static VolumeInfo volInfo;

        private static string currentTrackPath;

        #endregion

        #region Properties

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
            status = PlaybackStatus.Playing;
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
        /// This method starts the playback from stopped or paused.
        /// </summary>
        public static void Play()
        {
            if (status != PlaybackStatus.Playing)
                status = PlaybackStatus.Playing;
        }

        /// <summary>
        /// This method either starts or pauses the playback depending on the curren status.
        /// </summary>
        public static void Pause()
        {
            if (status == PlaybackStatus.Playing)
                status = PlaybackStatus.Paused;
            else if (status == PlaybackStatus.Paused)
                status = PlaybackStatus.Playing;
        }

        /// <summary>
        /// This method stops the playback.
        /// </summary>
        public static void Stop()
        {
            if (status == PlaybackStatus.Playing || status == PlaybackStatus.Paused)
                status = PlaybackStatus.Stopped;
        }
        
        /// <summary>
        /// This method jumps to the next song in the playlist.
        /// </summary>
        public static void Next()
        {

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
                    connection.SendMessage("QUEUESTART#" + playQueue.Length);
                    for (int pos = 0; pos < playQueue.Length; pos++)
                    {
                        connection.SendMessage("QUEUE#" + pos + "|" + playQueue[pos]);
                    }
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
                switch (status)
                {
                    case PlaybackStatus.Playing:
                        if (playlist.Count <= 0)
                            waitHandle.WaitOne();
                        waitHandle.Reset();
                        string song = playlist.Dequeue();
                        currentTrackPath = song;
                        WT32.BroadcastMessage("REMOVE#0");
                        //test
                        WT32.BroadcastMessage("PLAYING#"+song);
                        Debug.Print("BlueConePlayer: Playing \"" + song + "\"");
                        LED.State = LEDState.Playing;
                        file = File.OpenRead(song);
                        //long fileSize = file.Length;
                        //ushort byteRate;
                        //ushort decodeTime;
                        //float pos;
                        do
                        {
                            size = file.Read(buffer, 0, buffer.Length);
                            VS1053.SendData(buffer);
                            //decodeTime = VS1053.GetDecodeTime();
                            //byteRate = VS1053.GetByteRate();
                            //pos = ((float)(byteRate * decodeTime) / (float)fileSize) * 100;
                            //Debug.Print("Position in track: " + pos + "%");
                        } while (size > 0);
                        VS1053.Reset();
                        currentTrackPath = null;
                        file.Close();
                        file.Dispose();
                        LED.State = LEDState.Ready;
                        break;
                    case PlaybackStatus.Paused:
                        Thread.Sleep(10);
                        break;
                    case PlaybackStatus.Stopped:
                        Thread.Sleep(10);
                        break;
                    default:
                        break;
                }
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

        private enum PlaybackStatus
        {
            Playing = 0,
            Paused,
            Stopped
        }
    }
}
