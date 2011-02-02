using System;
using Microsoft.SPOT;
using System.Threading;
using BlueCone.Bluetooth;

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

        private static bool isPlaying;
        private static bool isPaused;

        private static Thread playThread;
        private static Playlist playlist;

        #endregion

        #region Methods

        #region Public Methods

        public static void Initialize()
        {
            isPlaying = false;
            isPaused = false;
            playThread = new Thread(PlayMusic);
            playlist = new Playlist();
            Debug.Print("BlueConePlayer initialized.");
        }

        public static void Play()
        {
            if (!isPlaying)
            {
                isPlaying = !isPlaying;
            }
            else if (isPlaying && isPaused)
            {
                isPaused = !isPaused;
            }
        }

        public static void Pause()
        {
            if (isPlaying)
            {
                isPaused = !isPaused;
            }
        }

        public static void Stop()
        {

        }

        public static void Next()
        {

        }

        public static void Previous()
        {

        }

        public static void AddTrack(string path, Link link)
        {
            playlist.Enqueue(path, link);
            WT32.BroadcastMessage("QUEUE#" + path);
        }

        #endregion

        #region Private Methods

        private static void PlayMusic()
        {

        }

        #endregion

        #endregion
    }
}
