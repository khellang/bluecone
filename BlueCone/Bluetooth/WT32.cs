using System;
using Microsoft.SPOT;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;
using System.IO;
using BlueCone.Mp3;
using BlueCone.Utils;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
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

        private static bool isInitialized = false;
        private static SerialPort bluetooth;
        private static byte[] sendBuffer;
        private static Hashtable connections;
        private static Thread readDataThread;

        private static OutputPort RESET = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di8, false);

        public static event MessageReceivedEventHandler MessageReceived;

        #endregion

        #region Properties

        public static Hashtable Connections
        {
            get
            {
                return connections;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Method for initializing the module. Must be called before ANY other method.
        /// </summary>
        public static void Initialize()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                bluetooth = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
                bluetooth.Open();
                readDataThread = new Thread(ReadData);
                readDataThread.Priority = ThreadPriority.Lowest;
                readDataThread.Start();
                connections = new Hashtable(7);
                // Set main settings.
                sendBuffer = Encoding.UTF8.GetBytes("SET CONTROL MUX 1\r\n");
                bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
                ExcecuteCommand("SET PROFILE SPP ON");
                ExcecuteCommand("SET BT NAME BlueCone");
                ExcecuteCommand("SET BT PAGEMODE 3 2000 1");
                ExcecuteCommand("SET BT AUTH * " + Settings.PairingKey);
                Thread.Sleep(100);
                Reset();
            }
        }

        /// <summary>
        /// Method for sending a message to the link specified in the <see cref="BluetoothMessage"/>.
        /// </summary>
        /// <param name="message">The message object with command and receiving link.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendMessage(BluetoothMessage message)
        {
            if (!isInitialized)
                throw new InvalidOperationException("WT32: Please call Initialize() before sending messages.");
            
            sendBuffer = Multiplexing.MUX(message);
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("WT32: Message \"" + message.Command.Trim() + "\" sent to link " + message.Link + ".");
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
            if (!isInitialized)
                throw new InvalidOperationException("WT32: Please call Initialize() before sending commands.");

            sendBuffer = Multiplexing.MUX(new BluetoothMessage(Link.Control, command));
            bluetooth.Write(sendBuffer, 0, sendBuffer.Length);
            Debug.Print("WT32: Command \"" + command + "\" sent.");
        }

        #endregion

        #region Private Methods

        private static void ReadData()
        {
            while (true)
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
                        try
                        {
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
                        catch (Exception)
                        {
                            Debug.Print("WT32: Unable to read incoming message.");
                        }
                    }
                    bytesRead++;
                }
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
            ExcecuteCommand("RESET");
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
                    string friendlyName = "";
                    for (int i = 2; i < tmp.Length; i++)
                    {
                        friendlyName += tmp[i] + " ";
                    }
                    Debug.Print("WT32: Friendly name of " + tmp[1] + " is " + tmp[2]);
                    break;
                case "READY.":
                    Debug.Print("WT32: Initialized Bluetooth Module.");
                    break;
                default:
                    Debug.Print("WT32: " + command);
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
            Debug.Print("WT32: Connection received from " + address + ", Link : " + link + ".");
            BlueConePlayer.SendTracks(newConnection);
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
            Debug.Print("WT32: Connection " + link + " removed");
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
