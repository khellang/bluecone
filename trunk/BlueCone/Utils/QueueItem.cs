using System;
using Microsoft.SPOT;

namespace BlueCone.Utils
{
    public class QueueItem : IComparable
    {
        private string path;
        private int priority;

        public QueueItem(string path, int priority)
        {
            this.path = path;
            this.priority = priority;
        }

        #region getters
        public int Priority
        {
            get { return priority;}
        }

        public string PATH
        {
            get { return path; }
        }
#endregion

        #region methods

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
