using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Mp3
{
    /// <summary>
    /// Driver class for the VS1053 MP3 decoder.
    /// </summary>
    public static class VS1053
    {
        #region Fields

        private static InputPort DREQ;

        private static SPI spi;
        private static SPI.Configuration dataConfig;
        private static SPI.Configuration cmdConfig;

        private static byte[] block = new byte[32];
        private static byte[] cmdBuffer = new byte[4];

        #endregion

        #region Constants

        // Values
        const ushort SM_SDINEW = 0x800;
        const ushort SM_RESET = 0x04;
        const ushort SM_CANCEL = 0x08;

        // Registers
        const int SCI_MODE = 0x00;
        const int SCI_VOL = 0x0B;
        const int SCI_CLOCKF = 0x03;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Method for initializing the decoder. Must be called before ANY other method.
        /// </summary>
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
                Debug.Print("VS1053 initialized.");
            }
        }

        /// <summary>
        /// Method for setting the volume.
        /// </summary>
        /// <param name="volume">Volume. (1.0 is MAX, 0.0 is MIN)</param>
        public static void SetVolume(double volume)
        {
            byte vol = (byte)(255 * volume);
            SetVolume(vol, vol);
        }

        /// <summary>
        /// Method for sending data to the decoder. Sends in chunks of 32byte.
        /// </summary>
        /// <param name="data">The data to decode. (This should not be too big!)</param>
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Metod for setting the volume on the left and right channel.
        /// <remarks>0 for min volume, 255 for max</remarks>
        /// </summary>
        /// <param name="left_channel">The left channel.</param>
        /// <param name="right_channel">The right channel.</param>
        private static void SetVolume(byte left_channel, byte right_channel)
        {
            CommandWrite(SCI_VOL, (ushort)((255 - left_channel) << 8 | (255 - right_channel)));
        }

        /// <summary>
        /// Method for doing a soft reset of the module.
        /// </summary>
        private static void Reset()
        {
            while (DREQ.Read() == false) ;
            CommandWrite(SCI_MODE, (ushort)(CommandRead(SCI_MODE) | SM_RESET));
            Thread.Sleep(1);
            while (DREQ.Read() == false) ;
            Thread.Sleep(100);
        }

        /// <summary>
        /// Method for writing a command to the decoder.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
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

        /// <summary>
        /// Method for reading a command from the decoder.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>The data read.</returns>
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

        #endregion

        #endregion
    }
}
