using System;
using Microsoft.SPOT;
using System.Collections;

namespace BlueCone.Utils
{
    public class Playlist
    {
        #region members
        private Hashtable myTable;
        private const int STARTPRIORITY = 0;
        private static int priorityValue;
        #endregion

        public Playlist()
        {
            Clear();
        }

      #region methods
        public void Clear()
        {
            myTable = new Hashtable();
        }

        public void Enqueue(string path, string MACAddress)
        {
            MACRegistered(MACAddress);

           
        }

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

        private int GetPriority(string MACAddress)
        {
            priorityValue = (int)myTable[MACAddress];
            myTable[MACAddress] = priorityValue + 1;
            return priorityValue;
        }

      #endregion




    }
}
