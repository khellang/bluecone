using System;
using Microsoft.SPOT;
using System.IO;
using System.Text;

namespace BlueCone.Utils.ID3
{
    public class ID3TagStream : FileStream
    {
        public ID3TagStream(string path, FileMode mode) : base(path, mode) { }

        /// <summary>
        /// Read string from current FileStream
        /// </summary>
        /// <param name="MaxLength">Maximum length that can read from stream</param>
        /// <param name="TEncoding">TextEcoding to read from Stream</param>
        /// <param name="DetectEncoding">Can method recognize encoding of text from Encoding inicators</param>
        /// <returns>string readed from current FileStream</returns>
        public string ReadText(int MaxLength, ref int ReadedLength)
        {
            if (MaxLength <= 0)
                return "";
            long Pos = base.Position;

            MemoryStream MStream = new MemoryStream();
            byte Buf;
            while (MaxLength > 0)
            {
                Buf = ReadByte(); // Read First/Next byte from stream

                if (Buf != 0) // if it's data byte
                    MStream.WriteByte(Buf);
                else // if Buf == 0
                {
                    byte Temp = ReadByte();
                    if (Temp == 0)
                        break;
                    else
                    {
                        MStream.WriteByte(Buf);
                        MStream.WriteByte(Temp);
                        MaxLength--;
                    }
                }
                MaxLength--;
            }

            if (MaxLength < 0)
                base.Position += MaxLength;

            ReadedLength -= (int)(base.Position - Pos);

            return new string(Encoding.UTF8.GetChars(MStream.ToArray()));
        }

        public string ReadText(int MaxLength)
        {
            int i = 0;
            return ReadText(MaxLength, ref i);
        }

        /// <summary>
        /// Read a byte from current FileStream
        /// </summary>
        /// <returns>Readed byte</returns>
        public new byte ReadByte()
        {
            byte[] RByte = new byte[1];

            // Use read method of FileStream instead of ReadByte
            // Becuase ReadByte return a SIGNED byte as integer
            // But what we want here is unsigned byte
            base.Read(RByte, 0, 1);

            return RByte[0];
        }   

        /// <summary>
        /// Read data from specific FileStream and return it as MemoryStream
        /// </summary>
        /// <param name="Length">Length that must read</param>
        /// <returns>MemoryStream readed from FileStream</returns>
        public MemoryStream ReadData(int Length)
        {
            MemoryStream ms;
            byte[] Buf = new byte[Length];
            base.Read(Buf, 0, Length);
            ms = new MemoryStream();
            ms.Write(Buf, 0, Length);

            return ms;
        }

        /// <summary>
        /// Indicate if file contain ID3v2 information
        /// </summary>
        /// <returns>true if contain otherwise false</returns>
        public bool HaveID3v2()
        {
            /* if the first three characters in begining of a file
             * be "ID3". that mpeg file contain ID3v2 information
             */
            string Iden = ReadText(3);
            if (Iden == "ID3")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Indicate if current File have ID3v1
        /// </summary>
        public bool HaveID3v1()
        {
            base.Seek(-128, SeekOrigin.End);
            string Tag = ReadText(3);
            if (Tag == "TAG")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Read ID3 version from current file
        /// </summary>
        /// <returns>Version contain ID3v2 version</returns>
        public Version ReadVersion()
        {
            return new Version((int)ReadByte(), (int)ReadByte());
        }

        /// <summary>
        /// Read ID3 Size
        /// </summary>
        /// <returns>ID3 Length size</returns>
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
