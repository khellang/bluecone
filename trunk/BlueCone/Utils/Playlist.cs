using System;
using Microsoft.SPOT;
using System.Collections;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public class Playlist
    {
        #region Fields

        private Hashtable myTable;
        private PriorityQueue thePlaylist;
        private const int STARTPRIORITY = 0;
        private static int priorityValue;

        #endregion

        #region Ctor

        public Playlist()
        {
            Clear();
        }

        #endregion

        #region Methods

        #region Public Methods

        public void Clear()
        {
            myTable = new Hashtable();
            thePlaylist = new PriorityQueue();
        }

        public void Enqueue(string path, string MACAddress)
        {
            MACRegistered(MACAddress);
            GetPriority(MACAddress);
            thePlaylist.Add(new QueueItem(path, priorityValue));
        }

        public string Dequeue()
        {
            string tmp = thePlaylist.Remove().ToString();
            return tmp;
        }

        #endregion

        #region Private Methods

        private Boolean MACRegistered(string MACAddress)
        {
            if (myTable.Contains(MACAddress))
                return true;
            else
            {
                myTable.Add(MACAddress, STARTPRIORITY);
                return false;
            }
        }

        private void GetPriority(string MACAddress)
        {
            priorityValue = (int)myTable[MACAddress];
            myTable[MACAddress] = priorityValue + 1;
    //        return priorityValue;
        }

        #endregion

        #endregion
    }
}
