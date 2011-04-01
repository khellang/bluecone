using System;
using System.Collections;
using System.Runtime.CompilerServices;
using BlueCone.Utils;
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

        public bool PriorityOn
        {
            get { return thePlaylist.PriorityOn; }
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
        public int Enqueue(string path, string macID)
        {
            LinkRegistered(macID);
            GetPriority(macID);
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

        public void UsePriority(bool b)
        {
            thePlaylist.UsePriority(b);
        }

        #endregion

        #region Private Methods

        private Boolean LinkRegistered(string macID)
        {
            if (myTable.Contains(macID))
                return true;
            else
            {
                myTable.Add(macID, STARTPRIORITY);
                return false;
            }
        }

        private void GetPriority(string macID)
        {
            priorityValue = (int)myTable[macID]; // **Prøver med static variabel her mtp. garbage collector, må kanskje endres**
            myTable[macID] = priorityValue + 1;
            //        return priorityValue;   
        }

        #endregion

        #endregion
    }
}
