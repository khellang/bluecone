using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;

namespace BlueCone.Drivers
{
    static public class MP3
    {
        static private InputPort DREQ;

        static SPI spi;
        static private SPI.Configuration dataConfig;
        static private SPI.Configuration cmdConfig;

        // Values
        const ushort SM_SDINEW = 0x800;
        const ushort SM_RESET = 0x04;
        const ushort SM_CANCEL = 0x08;

        // Registers
        const int SCI_MODE = 0x00;
        const int SCI_VOL = 0x0B;
        const int SCI_CLOCKF = 0x03;

        private static byte[] block = new byte[32];
        private static byte[] cmdBuffer = new byte[4];

        public static void Initialize()
        {
            SPI.SPI_module spi_module;
            spi_module = SPI.SPI_module.SPI1;

            dataConfig = new SPI.Configuration((Cpu.Pin)FEZ_Pin.Digital.Di2, false, 0, 0, false, true, 2000, spi_module);
            cmdConfig = new SPI.Configuration((Cpu.Pin)FEZ_Pin.Digital.Di9, false, 0, 0, false, true, 2000, spi_module);
            DREQ = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di3, false, Port.ResistorMode.PullUp);

            spi = new SPI(dataConfig);

            Reset();

            CommandWrite(SCI_MODE, SM_SDINEW);
            CommandWrite(SCI_CLOCKF, 0x98 << 8);
            CommandWrite(SCI_VOL, 0x0101);

            if (CommandRead(SCI_VOL) != (0x0101))
            {
                throw new Exception("Failed to initialize MP3 Decoder.");
            }
            else
            {
                Debug.Print("Mp3 decoder initialized successfully!");
            }

        }

        public static void SetVolume(byte left_channel, byte right_channel)
        {
            CommandWrite(SCI_VOL, (ushort)((255 - left_channel) << 8 | (255 - right_channel)));
        }

        private static void Reset()
        {
            while (DREQ.Read() == false) ;
            CommandWrite(SCI_MODE, (ushort)(CommandRead(SCI_MODE) | SM_RESET));
            Thread.Sleep(1);
            while (DREQ.Read() == false) ;
            Thread.Sleep(100);
        }

        private static void CommandWrite(byte address, ushort data)
        {
            while (DREQ.Read() == false)
                Thread.Sleep(1);

            spi.Config = cmdConfig;
            cmdBuffer[0] = 0x02;
            cmdBuffer[1] = address;
            cmdBuffer[2] = (byte)(data >> 8);
            cmdBuffer[3] = (byte)data;

            spi.Write(cmdBuffer);

        }

        private static ushort CommandRead(byte address)
        {
            ushort temp;

            while (DREQ.Read() == false)
                Thread.Sleep(1);

            spi.Config = cmdConfig;
            cmdBuffer[0] = 0x03;
            cmdBuffer[1] = address;
            cmdBuffer[2] = 0;
            cmdBuffer[3] = 0;

            spi.WriteRead(cmdBuffer, cmdBuffer, 2);

            temp = cmdBuffer[0];
            temp <<= 8;

            temp += cmdBuffer[1];

            return temp;
        }

        public static void SendData(byte[] data)
        {
            int size = data.Length - data.Length % 32;

            spi.Config = dataConfig;
            for (int i = 0; i < size; i += 32)
            {

                while (DREQ.Read() == false)
                    Thread.Sleep(1);

                Array.Copy(data, i, block, 0, 32);

                spi.Write(block);
            }
        }
    }
}
