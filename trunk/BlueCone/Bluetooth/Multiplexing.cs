using System.Text;
using System;
using BlueCone.Utils;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Bluetooth
{
    public static class Multiplexing
    {
        #region Static Methods

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
            int messageLength = messageBuffer.Length;
            int pos = 0;
            byte[] frameBuffer = new byte[5 + messageLength];
            frameBuffer[pos++] = 0xBF;           // SOF (Always 0xBF)
            frameBuffer[pos++] = (byte)message.Link;     // Link (0xFF for control)
            frameBuffer[pos++] = 0x00;           // Flags (Always 0x00)
            frameBuffer[pos++] = (byte)messageLength;      // Length (Data length)
            for (int i = 0; i < messageLength; i++)
                frameBuffer[pos++] = messageBuffer[i]; // Data
            frameBuffer[pos++] = (byte)((byte)message.Link ^ 0xFF);   //nLink (Link XOR 0xFF)
            return frameBuffer;
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
            int messageLength = bytesReceived[3];
            int pos = 0;
            byte[] messageBuffer = new byte[messageLength];
            for (int i = 0; i < messageLength; i++)
                messageBuffer[i] = bytesReceived[pos++];
            message.Command = new string(Encoding.UTF8.GetChars(messageBuffer)).Trim();
            return message;
        }

        #endregion
    }
}
