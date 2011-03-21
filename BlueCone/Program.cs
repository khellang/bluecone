﻿using BlueCone.Utils;
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
            Debug.EnableGCMessages(false);
            Settings.Load();
            LED.Initialize();
            VS1053.Initialize();
            BlueConePlayer.Initialize();
            Thread.Sleep(500);
            WT32.Initialize();
            WT32.MessageReceived += new MessageReceivedEventHandler(BlueConeMessageReceived);
            Thread.Sleep(Timeout.Infinite);
        }

        private static void BlueConeMessageReceived(BluetoothMessage message)
        {
            tmp = message.Command.Split('#');
            switch (tmp[0].Trim())
            {
                case "NEXT":
                    //VS1053.StopPlayback();
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
                case "PREV":
                    Debug.Print("PREV");
                    break;
                case "ADD": // ADD#PATH
                    BlueConePlayer.AddTrack(tmp[1].Trim(), message.Link);
                    break;
                case "VOLUP":
                    VS1053.VolUp();
                    break;
                case "VOLDOWN":
                    VS1053.VolDown();
                    break;
                case "MASTER":
                    if (tmp[1].Trim() == Settings.MasterPassword)
                        WT32.SendMessage(new BluetoothMessage(message.Link, "MASTER#OK"));
                    else
                        WT32.SendMessage(new BluetoothMessage(message.Link, "MASTER#ERR"));
                    break;
                case "QUEUEREMOVE":
                    int i = BlueConePlayer.RemoveTrack(tmp[1].Trim());
                    WT32.BroadcastMessage("REMOVE#" + i.ToString());
                    Debug.Print("QueueRemove: " + tmp[1].Trim());
                    break;
                default:
                    Debug.Print(message.Command + ", Link: " + message.Link);
                    break;
            }
            message.Dispose();
        }     
    }
}
