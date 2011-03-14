using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;

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

        private static float currentVol = 10f;

        #endregion

        #region Constants

        #region SCI Modes (8.7.1 in datasheet)

        const ushort SM_RESET = 0x0004;
        const ushort SM_CANCEL = 0x0008;
        const ushort SM_SDINEW = 0x0800;

        #endregion

        #region SCI Registers (8.7 in datasheet)

        const int SCI_MODE = 0x00;
        const int SCI_STATUS = 0x01;
        const int SCI_BASS = 0x02;
        const int SCI_CLOCKF = 0x03;
        const int SCI_DECODE_TIME = 0x04;
        const int SCI_AUDATA = 0x05;
        const int SCI_WRAM = 0x06;
        const int SCI_WRAMADDR = 0x07;
        const int SCI_HDAT0 = 0x08;
        const int SCI_HDAT1 = 0x09;
        const int SCI_VOL = 0x0B;

        #endregion

        #region WRAM Parameters (9.11.1 in datasheet)

        const ushort para_chipID0 = 0x1E00;
        const ushort para_chipID1 = 0x1E01;
        const ushort para_version = 0x1E02;
        const ushort para_playSpeed = 0x1E04;
        const ushort para_byteRate = 0x1E05;
        const ushort para_endFillByte = 0x1E06;
        const ushort para_posMsec0 = 0x1E27;
        const ushort para_posMsec1 = 0x1E28;

        #endregion

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

            SCIWrite(SCI_MODE, SM_SDINEW);
            SCIWrite(SCI_CLOCKF, 0x98 << 8);
            SCIWrite(SCI_VOL, 0x2828);

            //ClearPlayback();

            if (SCIRead(SCI_VOL) != (0x2828))
            {
                throw new Exception("VS1053: Failed to initialize MP3 Decoder.");
            }
            else
            {
                Debug.Print("VS1053: Initialized MP3 Decoder.");
            }
        }

        /// <summary>
        /// Method for adjusting the volume up.
        /// </summary>
        public static void VolUp()
        {
            if (currentVol <= 14)
            {
                currentVol += 1f;
                SetVolume(currentVol);
            }
        }

        /// <summary>
        /// Method for adjusting the volume down
        /// </summary>
        public static void VolDown()
        {
            if (currentVol >= 1)
            {
                currentVol -= 1f;
                SetVolume(currentVol);
            }
        }

        /// <summary>
        /// Method for setting the volume.
        /// </summary>
        /// <param name="volume">Volume. (1.0 is MAX, 0.0 is MIN)</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SetVolume(float volume)
        {
            currentVol = volume;
            SetVolume((byte)((8 * volume)+135));
            Debug.Print("VS1053: Volume changed to " + volume + ".");
        }

        /// <summary>
        /// Method for sending data to the decoder. Sends in chunks of 32byte.
        /// </summary>
        /// <param name="data">The data to decode. (This should not be too big!)</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SendData(byte[] data)
        {
            int size = data.Length - data.Length % 32;

            spi.Config = dataConfig;
            for (int i = 0; i < size; i += 32)
            {
                while (!DREQ.Read())
                    Thread.Sleep(1);

                Array.Copy(data, i, block, 0, 32);

                spi.Write(block);
            }
            //Debug.Print("VS1053: " + data.Length + " bytes of data sent. Decode time: " + SCIRead(SCI_DECODE_TIME));
        }

        #endregion

        #region Private Methods

        private static void ClearPlayback()
        {
            Debug.Print("ClearPlayback");
            uint endFillByte = WRAMRead(para_endFillByte);
            do
                for (int n = 0; n < 2052; n++) SDIWrite((byte)(0xFF & endFillByte));
            while (SCIRead(SCI_HDAT0) != 0 && SCIRead(SCI_HDAT1) != 0);
        }

        /// <summary>
        /// Method from reading from WRAM.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static ushort WRAMRead(ushort address)
        {
            ushort tmp1, tmp2;
            SCIWrite(SCI_WRAMADDR, address);
            tmp1 = SCIRead(SCI_WRAM);
            SCIWrite(SCI_WRAMADDR, address);
            tmp2 = SCIRead(SCI_WRAM);
            if (tmp1 == tmp2) return tmp1;
            SCIWrite(SCI_WRAMADDR, address);
            tmp1 = SCIRead(SCI_WRAM);
            if (tmp1 == tmp2) return tmp1;
            SCIWrite(SCI_WRAMADDR, address);
            tmp1 = SCIRead(SCI_WRAM);
            if (tmp1 == tmp2) return tmp1;
            return tmp1;
        }

        /// <summary>
        /// Metod for setting the volume on the left and right channel.
        /// <remarks>0 for min volume, 255 for max</remarks>
        /// </summary>
        /// <param name="left_channel">The left channel.</param>
        /// <param name="right_channel">The right channel.</param>
        private static void SetVolume(byte volume)
        {
            ushort vol = (ushort)(((255 - volume) << 8) | (255 - volume));
            while (!DREQ.Read())
                Thread.Sleep(1);
            SCIWrite(SCI_VOL, vol);
           // SCIWrite(SCI_WRAMADDR, 0xC001);
           // SCIWrite(SCI_WRAM, vol);
        }

        /// <summary>
        /// Method for doing a soft reset of the module.
        /// </summary>
        private static void Reset()
        {
            while (!DREQ.Read());
            SCIWrite(SCI_MODE, (ushort)(SM_SDINEW | SM_RESET));
            Thread.Sleep(1);
            while (!DREQ.Read());
            Thread.Sleep(100);
        }

        private static void SDIWrite(byte datum)
        {
            while (!DREQ.Read()) ;
            spi.Write(new byte[] { datum });
        }

        /// <summary>
        /// Method for writing a command to the decoder.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        private static void SCIWrite(byte address, ushort data)
        {
            while (!DREQ.Read())
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
        private static ushort SCIRead(byte address)
        {
            ushort temp;

            while (!DREQ.Read())
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
