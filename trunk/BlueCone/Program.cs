using BlueCone.Utils;
using Microsoft.SPOT;
using System;
using System.Threading;
using System.IO.Ports;
using System.Text;
using BlueCone.Bluetooth;
using BlueCone.Mp3;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone
{
    public class Program
    {
        static string[] tmp;

        public static void Main() 
        {
            WT32.Initialize();
            BlueConePlayer.Initialize();
            WT32.MessageReceived += new MessageReceivedEventHandler(WT32_MessageReceived);
            Debug.EnableGCMessages(true);
            Thread.Sleep(Timeout.Infinite);
        }

        static void WT32_MessageReceived(BluetoothMessage message)
        {
            tmp = message.Command.Split('#');
            switch (tmp[0])
            {
                case "NEXT":
                    break;
                case "PLAY":
                    break;
                case "PAUSE":
                    break;
                case "STOP":
                    break;
                case "PREVIOUS":
                    break;
                case "ADD": // ADD#PATH
                    BlueConePlayer.AddTrack(tmp[1]);
                    break;
                default:
                    Debug.Print(message.Command + ", Link: " + message.Link);
                    break;
            }
            message.Dispose();
            //Debug.GC(true);
        }     
    }
}
