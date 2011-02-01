using BlueCone.Utils;
using Microsoft.SPOT;
using System;
using System.Threading;
using System.IO.Ports;
using System.Text;
using BlueCone.Bluetooth;

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
            WT32.MessageReceived += new MessageReceivedEventHandler(WT32_MessageReceived);
            Debug.EnableGCMessages(true);
            Thread.Sleep(Timeout.Infinite);
        }

        static void WT32_MessageReceived(BluetoothMessage message)
        {
            tmp = message.Command.Split(' ');
            switch (tmp[0])
            {
                case "GA":
                    string[] testData = new string[]
                    { "GA-6-STI-Metallica-Best Of-Wherever i may roam-", "STI-Metallica-Best Of-Master of puppets-",
                        "STI-Metallica-Best Of-So what?-", "STI-Metallica-Best Of-Hero of the day-",
                        "STI-Metallica-Best Of-Enter sandman-", "STI-Metallica-Best Of-Sad but true-" };
                    BluetoothMessage msg = new BluetoothMessage();
                    foreach (string track in testData)
                    {
                        msg = new BluetoothMessage(message.Link, track);
                        WT32.SendMessage(msg);
                    }
                    msg.Dispose();
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
