using System;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave V�ren 2011
//      Av Terje Knutsen, Stein Arild H�iland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public class PriorityQueue
    {
        #region Fields

        private QueueItem[] theQueue;
        private int count;
        private const int DEFAULTCAPACITY = 8;
        private const int NOITEMS = -1;

        #endregion

        #region Ctor

        public PriorityQueue()
        {
            Clear();
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return count; }
        }
    
        #endregion

        #region Methods

        #region Public Methods

        public void Clear()
        {
            theQueue = new QueueItem[DEFAULTCAPACITY];
            count = NOITEMS;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(QueueItem qi)
        {
            count++;
            if (count == theQueue.Length)
                DoubleQueue();
            theQueue[count] = qi;
            BubbleUp();
        }

        public QueueItem Remove()
        {
            if (count < 0)
                return new QueueItem("****** tom k�!! ********", 0); // kun for � vise at k�en er tom, m� bytte dette ut
            QueueItem tmp = theQueue[count];
            theQueue[count] = null;
            count--;
            return tmp;
        }

        #endregion

        #region Private Methods
        private void BubbleUp()
        {
            int i = count;
            while (i - 1 >= 0 && theQueue[i].CompareTo(theQueue[i - 1]) >= 0)
            {
                QueueItem tmp = theQueue[i - 1];
                theQueue[i - 1] = theQueue[i];
                theQueue[i] = tmp;
                i--;
            }
        }

        private void DoubleQueue()
        {
            QueueItem[] tmp = theQueue;
            theQueue = new QueueItem[theQueue.Length * 2 + 1];
            for (int i = 0; i < count; i++)
            {
                theQueue[i] = tmp[i];
            }
        }

        #endregion

        #endregion
    }
}
