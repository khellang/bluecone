using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.USBHost;
using GHIElectronics.NETMF.IO;
using System.IO;
using System.Text;
using System.IO.Ports;

namespace BlueCone
{
    public class Program
    {
        static PersistentStorage ps;

        public static void Main()
        {
            RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);
            RemovableMedia.Eject += new EjectEventHandler(RemovableMedia_Eject);

            USBHostController.DeviceConnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceConnectedEvent);
            USBHostController.DeviceDisconnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceDisconnectedEvent);

            Debug.Print("Waiting for USB device...");

            Thread.Sleep(Timeout.Infinite);
        }

        static void USBHostController_DeviceDisconnectedEvent(GHIElectronics.NETMF.USBHost.USBH_Device device)
        {
            Debug.Print("Device Disconnected");
        }

        static void USBHostController_DeviceConnectedEvent(GHIElectronics.NETMF.USBHost.USBH_Device device)
        {
            Debug.Print("Device connected, type: " + (USBH_DeviceType)device.TYPE);
            switch (device.TYPE)
            {
                case USBH_DeviceType.MassStorage:
                    Debug.Print("MassStorage detected");
                    Debug.Print("Mounting...");
                    ps = new PersistentStorage(device);
                    ps.MountFileSystem();
                break;
                default:
                    Debug.Print("Uncompatible device");
                break;
            }
        }

        static void RemovableMedia_Eject(object sender, MediaEventArgs e)
        {
            Debug.Print("MassStorage unmounted");
        }

        static void RemovableMedia_Insert(object sender, MediaEventArgs e)
        {
            Debug.Print("MassStorage mounted to " + e.Volume.RootDirectory);
            if (e.Volume.IsFormatted)
            {
                string[] files = Directory.GetFiles(e.Volume.RootDirectory);
                Debug.Print("Initializing mp3 decoder...");
                MP3.Initialize();
                Debug.Print("Looking for mp3/wma file...");
                FileStream file;
                byte[] chunk;
                int bytesRead;
                SerialPort bluetooth = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);
                bluetooth.Open();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Substring(files[i].LastIndexOf('.')).ToLower() == ".mp3" ||
                        files[i].Substring(files[i].LastIndexOf('.')).ToLower() == ".wma")
                    {
                        file = File.OpenRead(files[i]);
                        file.Seek(-128, SeekOrigin.End);
                        chunk = new byte[2048];
                        file.Read(chunk, 0, 3);
                        if (bytesToString(chunk).Equals("TAG"))
                        {
                            file.Read(chunk, 0, 30);
                            string title = bytesToString(chunk);
                            file.Read(chunk, 0, 30);
                            string artist = bytesToString(chunk);
                            file.Read(chunk, 0, 30);
                            string album = bytesToString(chunk);
                            byte[] songCommand = Encoding.UTF8.GetBytes(files[i] + "|" + artist + "|" + album + "|" + title + "\r\n");
                            bluetooth.Write(songCommand, 0, songCommand.Length);
                        }
                        file.Close();
                    }
                }
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Substring(files[i].LastIndexOf('.')).ToLower() == ".mp3" ||
                        files[i].Substring(files[i].LastIndexOf('.')).ToLower() == ".wma")
                    {
                        file = File.OpenRead(files[i]);
                        file.Seek(-128, SeekOrigin.End);
                        chunk = new byte[2048];
                        file.Read(chunk, 0, 3);
                        if (bytesToString(chunk).Equals("TAG"))
                        {
                            file.Read(chunk, 0, 30);
                            string title = bytesToString(chunk);
                            file.Read(chunk, 0, 30);
                            string artist = bytesToString(chunk);
                            Debug.Print("Found \"" + title + "\" by " + artist);
                        }
                        file.Seek(0, SeekOrigin.Begin);
                        Debug.Print("Playing file...");
                        do
                        {
                            bytesRead = file.Read(chunk, 0, 2048);
                            MP3.SendData(chunk);
                        } while (bytesRead > 0);
                        Debug.Print("Playback done!");
                        file.Close();
                        Debug.Print("Looking for new file...");
                    }
                }
            }
            else
            {
                Debug.Print("Storage is not formatted. Format on PC with FAT32/FAT16 first.");
            }
            ps.Dispose();
        }

        static string bytesToString(byte[] bytes)
        {
            return new string(Encoding.UTF8.GetChars(bytes)).Trim();
        }
    }
}
