using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Drivers.Bluetooth
{
    public class Connection
    {
        #region Fields

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
        }

        #endregion

        #region Methods

        #region Public Methods

        public void SendMessage(string message)
        {
            BluetoothMessage msg = new BluetoothMessage(this.link, message);
            WT32.SendMessage(msg);
            msg.Dispose();
        }

        #endregion

        #endregion
    }
}
