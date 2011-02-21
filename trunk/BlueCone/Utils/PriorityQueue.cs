using System;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public class PriorityQueue
    {
        #region Fields

        private QueueItem[] theQueue;
        private int count;
        private const int DEFAULTCAPACITY = 8;
        private const int NOITEMS = 0;
        private bool priorityOn = true;

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

        public bool PriorityOn
        {
            set { priorityOn = value; }
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
            if (theQueue.Length == count)
                DoubleQueue();
            theQueue[count] = qi;
            count++;
            if (count >= 2 && priorityOn)
                BubbleUp();

            /*
            count++;
            if (count == theQueue.Length)
                DoubleQueue();
            theQueue[count] = qi;
            BubbleUp();
             * */
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public QueueItem Remove()
        {
            /*
            if (count < 0)
                return new QueueItem("****** tom kø!! ********", 0); // kun for å vise at køen er tom, må bytte dette ut
            QueueItem tmp = theQueue[count];
            theQueue[count] = null;
            count--;
            return tmp;
             * */
            if (count < 0)
                return new QueueItem("****** tom kø!! ********", 0);
            QueueItem tmp = theQueue[0];
            MoveArray();
            count--;
            return tmp;
        }

        public string[] getQueue()
        {
            string[] tmp = new string[count];
            for (int i = 0; i < count; i++)
            {
                tmp[i] = theQueue[i].Path;
            }
            return tmp;
            /*
            string[] tmp = new string[count + 1];
            for (int i = count; i >= 0; i--)
            {
                tmp[count - i] = theQueue[i].Path;
            }
            return tmp;
             * */
        }

        #endregion

        #region Private Methods
        private void BubbleUp()
        {
            /*
            int i = count;
            while (i - 1 >= 0 && theQueue[i].CompareTo(theQueue[i - 1]) >= 0)
            {
                QueueItem tmp = theQueue[i - 1];
                theQueue[i - 1] = theQueue[i];
                theQueue[i] = tmp;
                i--;
            }
             * */
            int i = count - 1;
            while (i - 1 >= 0 && theQueue[i].CompareTo(theQueue[i - 1]) < 0)
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

        private void MoveArray()
        {
            for (int i = 1; i < count; i++)
            {
                QueueItem tmp = theQueue[i];
                theQueue[i - 1] = tmp;
            }
            theQueue[count - 1] = null;
        }

        #endregion

        #endregion
    }
}
