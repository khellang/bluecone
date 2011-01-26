using System.Text;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Drivers.Bluetooth
{
    public class MUXFrame
    {
        #region Fields

        private byte[] frame;

        #endregion

        #region Properties

        public string Command
        {
            get
            {
                byte[] dataBuffer = new byte[frame[3]];
                for (int i = 0, pos = 4; i < dataBuffer.Length; i++, pos++)
                {
                    dataBuffer[i] = frame[pos];
                }
                return new string(Encoding.UTF8.GetChars(dataBuffer));
            }
        }

        public Link Link
        {
            get
            {
                return (Link)frame[1];
            }
        }

        #endregion

        #region Ctor

        public MUXFrame(string command, Link link)
        {
            byte[] dataBuffer = Encoding.UTF8.GetBytes(command);
            int len = dataBuffer.Length, pos = 0;
            this.frame = new byte[5 + len];
            this.frame[pos++] = 0xBF;           // SOF (Always 0xBF)
            this.frame[pos++] = (byte)link;     // Link (0xFF for control)
            this.frame[pos++] = 0x00;           // Flags (Always 0x00)
            this.frame[pos++] = (byte)len;      // Length (Data length)
            // Data
            for (int i = 0; i < len; i++)
            {
                this.frame[pos++] = dataBuffer[i];
            }
            this.frame[pos++] = (byte)((byte)link ^ 0xFF);   //nLink (Link XOR 0xFF)
        }

        public MUXFrame(byte[] frame)
        {
            this.frame = frame;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Command;
        }

        #endregion
    }
}
