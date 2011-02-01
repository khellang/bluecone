using System;
using Microsoft.SPOT;
using System.Text;
using System.IO.Ports;
using System.Threading;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Drivers.Bluetooth
{
    #region Delegates

    public delegate void MessageReceivedEventHandler(BluetoothMessage message);

    #endregion

    public static class WT32
    {
        #region Fields

        private static SerialPort bluetooth;
        private static byte[] sendBuffer;

        public static event MessageReceivedEventHandler MessageReceived;

        #endregion

        #region Methods

        #region Public Methods

        public static void Initialize()
        {
            bluetooth = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            bluetooth.Open();
            bluetooth.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            Debug.Print("WT32 Initialized.");
        }

        public static void SendMessage(BluetoothMessage message)
        {
            sendBuffer = Multiplexing.MUX(message);
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("Message \"" + message.Command + "\" sent to link " + message.Link);
        }

        public static void ExcecuteCommand(string command)
        {
            sendBuffer = Multiplexing.MUX(new BluetoothMessage(Link.Control, command));
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("Command \"" + command + "\" sent");
        }

        #endregion

        #region Private Methods

        private static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesRead = 0;
            while (bytesRead < bluetooth.BytesToRead)
            {
                byte SOF = ReadByte(); // Read byte
                if (SOF == 0xBF) // Check for SOF
                {
                    byte link = ReadByte(); // Read link
                    ReadByte(); // Read flags - not used
                    int length = (int)ReadByte(); // Read data length
                    byte[] receiveBuffer = new byte[length];
                    bluetooth.Read(receiveBuffer, 0, receiveBuffer.Length); // Read data
                    string message = new string(Encoding.UTF8.GetChars(receiveBuffer)).Trim();
                    BluetoothMessage receivedMessage = new BluetoothMessage((Link)link, message);
                    if (MessageReceived != null)
                        MessageReceived(receivedMessage);
                }
                bytesRead++;
            }
        }

        private static byte ReadByte()
        {
            byte[] receiveBuffer = new byte[1];
            bluetooth.Read(receiveBuffer, 0, 1);
            return receiveBuffer[0];
        }

        #endregion

        #endregion
    }

    public enum Link : byte
    {
        One = 0x00,
        Two = 0x01,
        Three = 0x02,
        Four = 0x03,
        Five = 0x04,
        Six = 0x05,
        Seven = 0x06,
        Control = 0xFF
    }
}
