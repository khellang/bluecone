using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public class QueueItem : IComparable
    {
        #region Fields

        private string path;
        private int priority;

        #endregion

        #region Ctor

        public QueueItem(string path, int priority)
        {
            this.path = path;
            this.priority = priority;
        }

        #endregion

        #region Properties

        public int Priority
        {
            get { return priority; }
        }

        public string Path
        {
            get { return path; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return path;
        }

        public int Compare(Object obj)
        {
            if (obj is QueueItem)
            {
                QueueItem tmp = (QueueItem)obj;
                if (this.priority < tmp.Priority)
                    return -1;
                if (this.priority > tmp.priority)
                    return 1;
                else
                    return 0;
            }
            else
                throw new ArgumentException("object is not a QueueItem");
        }

        #endregion
    }
}
