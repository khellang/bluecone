using System;
using Microsoft.SPOT;
using System.Collections;
using BlueCone.Utils;
using BlueCone.Bluetooth;
using System.Threading;
using System.Runtime.CompilerServices;
//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Mp3
{
    public class Playlist
    {
        #region Fields

        private Hashtable myTable;
        private PriorityQueue thePlaylist;
        private const int STARTPRIORITY = 0;
        private static int priorityValue;

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                return this.thePlaylist.Count;
            }
        }

        #endregion

        #region Ctor

        public Playlist()
        {
            Clear();
        }

        #endregion

        #region Methods

        #region Public Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            myTable = new Hashtable();
            thePlaylist = new PriorityQueue();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int Enqueue(string path, Link link)
        {
            LinkRegistered(link);
            GetPriority(link);
            int pos = thePlaylist.Add(new QueueItem(path, priorityValue));
            return pos;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Dequeue()
        {
            string tmp = thePlaylist.Remove().ToString();
            return tmp;
        }

        public int Remove(string path)
        {
            return thePlaylist.Remove(path);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string[] GetPlaylist()
        {
            return thePlaylist.getQueue();
        }

        public void UsePriority()
        {
            thePlaylist.UsePriority();
        }

        #endregion

        #region Private Methods

        private Boolean LinkRegistered(Link link)
        {
            if (myTable.Contains(link))
                return true;
            else
            {
                myTable.Add(link, STARTPRIORITY);
                return false;
            }
        }

        private void GetPriority(Link link)
        {
            priorityValue = (int)myTable[link]; // **Prøver med static variabel her mtp. garbage collector, må kanskje endres**
            myTable[link] = priorityValue + 1;
    //        return priorityValue;   
        }

        #endregion

        #endregion
    }
}
