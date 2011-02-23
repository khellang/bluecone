using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Bluetooth
{
    /// <summary>
    /// This class represents a bluetooth connection to a unit.
    /// </summary>
    public class Connection : IDisposable
    {
        #region Fields

        private bool disposed = false;

        private string address;
        private Link link;
        private string friendlyName;

        #endregion

        #region Properties

        public string Address
        {
            get
            {
                return this.address;
            }
        }

        public Link Link
        {
            get
            {
                return this.link;
            }
        }

        public string FriendlyName
        {
            get
            {
                return this.friendlyName;
            }
            set
            {
                this.friendlyName = value;
            }
        }

        #endregion

        #region Ctor

        public Connection(string address, Link link)
        {
            this.address = address;
            this.link = link;
            this.friendlyName = "Unknown";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method for sending a message to the connected unit.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(string message)
        {
            BluetoothMessage msg = new BluetoothMessage(this.link, message + "\r\n");
            WT32.SendMessage(msg);
            msg.Dispose();
        }

        /// <summary>
        /// Method for closing the bluetooth connection.
        /// </summary>
        public void Close()
        {
            // TODO: Implementer denne metoden! :)
        }

        /// <summary>
        /// Method for disposing this connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (address != null)
                        address = null;
                    if (friendlyName != null)
                        friendlyName = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}
