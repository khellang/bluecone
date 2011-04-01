using System;
using System.Threading;
using BlueCone.Bluetooth;
using BlueCone.Mp3;
using BlueCone.Utils;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone
{
    public class Program
    {
        static string[] tmp;
        static InterruptPort button;

        public static void Main()
        {
            Debug.EnableGCMessages(false);
            Settings.Load();
            LED.Initialize();
            VS1053.Initialize();
            BlueConePlayer.Initialize();
            Thread.Sleep(500);
            WT32.Initialize();
            WT32.MessageReceived += new MessageReceivedEventHandler(BlueConeMessageReceived);
            button = new InterruptPort((Cpu.Pin)FEZ_Pin.Digital.Di6, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += new NativeEventHandler(button_OnInterrupt);
            Thread.Sleep(Timeout.Infinite);
        }

        static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            BlueConePlayer.Next();
        }

        private static void BlueConeMessageReceived(BluetoothMessage message)
        {
            tmp = message.Command.Split('#');
            switch (tmp[0].Trim())
            {
                case "NEXT":
                    BlueConePlayer.Next();
                    break;
                case "PLAY":
                    Debug.Print("PLAY");
                    break;
                case "PAUSE":
                    Debug.Print("PAUSE");
                    break;
                case "STOP":
                    Debug.Print("STOP");
                    break;
                case "PRI":
                    Debug.Print("PRI");
                    Debug.Print(tmp[1].Trim());
                    if (tmp[1].Trim() == "0")
                    {
                        BlueConePlayer.UsePriority(false);
                    }
                    else
                    {
                        BlueConePlayer.UsePriority(true);
                    }
                    break;
                case "ADD": // ADD#PATH
                    BlueConePlayer.AddTrack(tmp[1].Trim(), ((Connection)WT32.Connections[message.Link]).Address);
                    break;
                case "VOLUP":
                    VS1053.VolUp();
                    break;
                case "VOLDOWN":
                    VS1053.VolDown();
                    break;
                case "MASTER":
                    if (tmp[1].Trim() == Settings.MasterPassword)
                        WT32.SendMessage(new BluetoothMessage(message.Link, "MASTER#OK|" + BlueConePlayer.PriorityOn.ToString()));
                    else
                        WT32.SendMessage(new BluetoothMessage(message.Link, "MASTER#ERR"));
                    break;
                case "QUEUEREMOVE":
                    int i = BlueConePlayer.RemoveTrack(tmp[1].Trim());
                    WT32.BroadcastMessage("REMOVE#" + i);
                    Debug.Print("QueueRemove: " + tmp[1].Trim());
                    break;
                case "REQ_QUEUE":
                    BlueConePlayer.SendTracks((Connection)WT32.Connections[message.Link], false);
                    break;
                case "REQ_ALL":
                    BlueConePlayer.SendTracks((Connection)WT32.Connections[message.Link], true);
                    break;
                default:
                    Debug.Print(message.Command + ", Link: " + message.Link);
                    break;
            }
            message.Dispose();
        }
    }
}
