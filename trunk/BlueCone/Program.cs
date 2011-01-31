

using BlueCone.Utils;
using Microsoft.SPOT;
using System;
using System.Threading;
using System.IO.Ports;
using System.Text;
using BlueCone.Drivers.Bluetooth;
//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone
{
    public class Program
    {
        private static string[] tmp;

        public static void Main() 
        {
            WT32.Initialize();
            WT32.MessageReceived += new MessageReceivedEventHandler(WT32_MessageReceived);
            Thread.Sleep(Timeout.Infinite);
        }

        static void WT32_MessageReceived(BluetoothMessage message)
        {
            tmp = message.Command.Split(' ');
            switch (tmp[0])
            {
                case "RING":
                    Debug.Print("Tilkobling fra " + tmp[2] + ", Link: " + tmp[1]);
                    break;
                case "NO":
                    Debug.Print("Link " + tmp[2] + " frakoblet.");
                    break;
                case "NAME":
                    Debug.Print("Friendly name of " + tmp[1] + " is " + tmp[2]);
                    break;
                default:
                    Debug.Print(message.Command);
                    break;
            }
        }     
    }
}
