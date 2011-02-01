using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Drivers.Bluetooth
{
    /// <summary>
    /// This class represents a message to be sent to either a bluetooth link or the module itself.
    /// </summary>
    public class BluetoothMessage : IDisposable
    {
        #region Fields

        private bool disposed = false;

        private Link link;
        private string command;

        #endregion

        #region Properties

        public Link Link
        {
            get
            {
                return this.link;
            }
            set
            {
                this.link = value;
            }
        }

        public string Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        #endregion

        #region Ctor

        public BluetoothMessage(Link link, string command)
        {
            this.link = link;
            this.command = command;
        }

        public BluetoothMessage()
        {

        }

        ~BluetoothMessage()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

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
                    if (command != null)
                        command = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}
