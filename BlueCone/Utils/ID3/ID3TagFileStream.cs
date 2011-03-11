using System;
using System.IO;
using System.Text;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    public class ID3TagFileStream : FileStream
    {
        public ID3TagFileStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public ID3TagFileStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        public string ReadText(int length)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                int maxLength = length;
                byte readByte;
                while (maxLength > 0)
                {
                    readByte = ReadByte();
                    if (readByte != 0)
                        ms.WriteByte(readByte);
                    maxLength--;
                }
                if (maxLength < 0)
                    base.Position += maxLength;
                return new string(Encoding.UTF8.GetChars(ms.ToArray()));
            }
        }

        public new byte ReadByte()
        {
            byte[] byteRead = new byte[1];

            // Use read method of FileStream instead of ReadByte
            // Becuase ReadByte return a SIGNED byte as integer
            // But what we want here is unsigned byte
            base.Read(byteRead, 0, 1);

            return byteRead[0];
        } 

        public bool HaveID3v2()
        {
            string tag = ReadText(3);
            if (tag == "ID3")
                return true;
            else
                return false;
        }

        public bool HaveID3v1()
        {
            base.Seek(-128, SeekOrigin.End);
            string tag = ReadText(3);
            if (tag == "TAG")
                return true;
            else
                return false;
        }

        public Version ReadVersion()
        {
            return new Version((int)ReadByte(), (int)ReadByte());
        }

        public int ReadSize()
        {
            /* ID3 Size is like:
             * 0XXXXXXXb 0XXXXXXXb 0XXXXXXXb 0XXXXXXXb (b means binary)
             * the zero bytes must ignore, so we have 28 bits number = 0x1000 0000 (maximum)
             * it's equal to 256MB
             */
            int RInt;
            RInt = ReadByte() * 0x200000;
            RInt += ReadByte() * 0x4000;
            RInt += ReadByte() * 0x80;
            RInt += ReadByte();
            return RInt;
        }
    }
}
