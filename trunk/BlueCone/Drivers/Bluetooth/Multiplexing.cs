using System.Text;
using System;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Drivers.Bluetooth
{
    public static class Multiplexing
    {
        #region Methods

        /// <summary>
        /// Method for encapsulating a message in a MUX frame.
        /// </summary>
        /// <param name="message">The message to encapsulate.</param>
        /// <returns>Byte array of full MUX frame.</returns>
        public static byte[] MUX(BluetoothMessage message)
        {
            if (message.Command == null)
                throw new ArgumentException("Message is not complete. Check the Command property!");

            byte[] messageBuffer = Encoding.UTF8.GetBytes(message.Command);
            int len = messageBuffer.Length, pos = 0;
            byte[] frame = new byte[5 + len];
            frame[pos++] = 0xBF;           // SOF (Always 0xBF)
            frame[pos++] = (byte)message.Link;     // Link (0xFF for control)
            frame[pos++] = 0x00;           // Flags (Always 0x00)
            frame[pos++] = (byte)len;      // Length (Data length)
            for (int i = 0; i < len; i++)
                frame[pos++] = messageBuffer[i]; // Data
            frame[pos++] = (byte)((byte)message.Link ^ 0xFF);   //nLink (Link XOR 0xFF)
            return frame;
        }

        /// <summary>
        /// Method for decapsulating the message from a MUX frame.
        /// </summary>
        /// <param name="bytesReceived">The byte array received over the UART interface.</param>
        /// <returns>The decapsulated message.</returns>
        public static BluetoothMessage DeMUX(byte[] bytesReceived)
        {
            if (bytesReceived[0] != 0xBF)
                throw new ArgumentException("ByteArray is not a MUX-message");

            BluetoothMessage message = new BluetoothMessage();
            message.Link = (Link)bytesReceived[1];
            int dataLength = bytesReceived[3], pos = 0;
            byte[] data = new byte[dataLength];
            for (int i = 0; i < data.Length; i++)
                data[i] = bytesReceived[pos++];
            message.Command = new string(Encoding.UTF8.GetChars(data)).Trim();
            return message;
        }

        #endregion
    }

    public struct BluetoothMessage
    {
        public Link Link { get; set; }
        public string Command { get; set; }

        public BluetoothMessage(Link link, string command)
        {
            this.Link = link;
            this.Command = command;
        }
    }
}
