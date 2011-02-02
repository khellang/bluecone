using System;
using Microsoft.SPOT;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave V�ren 2011
//      Av Terje Knutsen, Stein Arild H�iland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Bluetooth
{
    #region Delegates

    public delegate void MessageReceivedEventHandler(BluetoothMessage message);

    #endregion

    /// <summary>
    /// Driver class for the WT32 bluetooth module.
    /// </summary>
    public static class WT32
    {
        #region Fields

        private static SerialPort bluetooth;
        private static byte[] sendBuffer;
        private static Hashtable connections;

        public static event MessageReceivedEventHandler MessageReceived;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Method for initializing the module. Must be called before ANY other method.
        /// </summary>
        public static void Initialize()
        {
            bluetooth = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
            bluetooth.Open();
            bluetooth.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            connections = new Hashtable(7);
            Debug.Print("WT32 Initialized.");
        }

        /// <summary>
        /// Method for sending a message to the link specified in the <see cref="BluetoothMessage"/>.
        /// </summary>
        /// <param name="message">The message object with command and receiving link.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendMessage(BluetoothMessage message)
        {
            if (!bluetooth.IsOpen)
                throw new InvalidOperationException("Please call Initialize() before sending messages.");
            
            sendBuffer = Multiplexing.MUX(message);
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("Message \"" + message.Command + "\" sent to link " + message.Link);
        }

        /// <summary>
        /// Method for broadcasting a message to all connected units.
        /// </summary>
        /// <param name="message">The message to send.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void BroadcastMessage(string message)
        {
            foreach (Connection connection in connections.Values)
            {
                connection.SendMessage(message);
            }
        }

        /// <summary>
        /// Method for excecuting a controlcommand on the module.
        /// </summary>
        /// <param name="command">The controlcommand. (See iWRAP documentation)</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        public static void ExcecuteCommand(string command)
        {
            if (!bluetooth.IsOpen)
                throw new InvalidOperationException("Please call Initialize() before sending messages.");

            sendBuffer = Multiplexing.MUX(new BluetoothMessage(Link.Control, command));
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("Command \"" + command + "\" sent");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method is called once the SerialPort receives any data.
        /// It calls either <see cref="HandleControlCommand"/> or the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <param name="sender">The sender object of this event.</param>
        /// <param name="e">The event arguments.</param>
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
                    if ((Link)link == Link.Control)
                        HandleControlCommand(message);
                    else
                    {
                        BluetoothMessage receivedMessage = new BluetoothMessage((Link)link, message);
                        if (MessageReceived != null)
                            MessageReceived(receivedMessage);
                    }
                }
                bytesRead++;
            }
        }

        /// <summary>
        /// Helper method for reading a simple byte from the SerialPort.
        /// </summary>
        /// <returns>The byte read.</returns>
        private static byte ReadByte()
        {
            byte[] receiveBuffer = new byte[1];
            bluetooth.Read(receiveBuffer, 0, 1);
            return receiveBuffer[0];
        }

        /// <summary>
        /// Method for doing a soft reset of the module.
        /// </summary>
        private static void Reset()
        {
            // TODO: Implementer denne metoden.
        }

        /// <summary>
        /// Method for handling local controlcommands from the module.
        /// </summary>
        /// <param name="command">The command received from the module.</param>
        private static void HandleControlCommand(string command)
        {
            string[] tmp = command.Split(' ');
            switch (tmp[0])
            {
                case "RING": // Enhet tilkoblet.
                    AddConnection(tmp[2], tmp[1]);
                    break;
                case "NO": // Enhet frakoblet.
                    RemoveConnection(tmp[2]);
                    break;
                case "NAME": // Mottatt "friendlyname"
                    Debug.Print("Friendly name of " + tmp[1] + " is " + tmp[2]);
                    break;
                    // TODO: Implementer resten av denne metoden.
            }
        }

        /// <summary>
        /// Helper method for adding a connection to the hashtable.
        /// </summary>
        /// <param name="address">The address of the connection.</param>
        /// <param name="link">The link of the connection. (NB! This will be converted to a Link enum)</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void AddConnection(string address, string link)
        {
            Link newLink = (Link)Convert.ToByte(link);
            Connection newConnection = new Connection(address, newLink);
            connections.Add(newLink, newConnection);
            Debug.Print("Connection received from " + address + ", Link : " + link);
        }

        /// <summary>
        /// Helper method for removing a connection from the hashtable.
        /// </summary>
        /// <param name="link">The link of the connection to remove. (NB! This will be converted to a Link enum)</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void RemoveConnection(string link)
        {
            Link oldLink = (Link)Convert.ToByte(link);
            if (connections.Contains(oldLink))
                connections.Remove(oldLink);
            Debug.Print("Connection " + link + " removed");
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Link enum.
    /// </summary>
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