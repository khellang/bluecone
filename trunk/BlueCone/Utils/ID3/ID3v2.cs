using System;
using System.IO;
using System.Collections;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    /// <summary>
    /// Partial ID3v2 Frame.
    /// </summary>
    public class ID3v2 : ID3Tag, IDisposable
    {
        #region Fields

        private bool disposed = false;

        private string path;
        private string title;
        private string artist;
        private string album;
        private bool isComplete;
        private Version version;
        private ArrayList frames;
        private Hashtable frameIDs;

        #endregion

        #region Properties

        public string Path
        {
            get { return this.path; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Artist
        {
            get { return this.artist; }
        }

        public string Album
        {
            get { return this.album; }
        }

        public bool IsComplete
        {
            get { return this.isComplete; }
        }

        public Version Version
        {
            get { return this.version; }
        }

        #endregion

        #region Ctor

        public ID3v2(string path)
        {
            this.path = path;
            this.frames = new ArrayList();
            this.frames.Add("TPE1");
            this.frames.Add("TALB");
            this.frames.Add("TIT2");
            this.frameIDs = new Hashtable();
            this.frameIDs.Add("TP1", "TPE1");
            this.frameIDs.Add("TAL", "TALB");
            this.frameIDs.Add("TT2", "TIT2");
            using (ID3TagFileStream fs = new ID3TagFileStream(path, FileMode.Open))
            {
                if (!fs.HaveID3v2())
                {
                    fs.Close();
                    isComplete = false;
                    return;
                }

                version = fs.ReadVersion(); // Read ID3v2 version
                // Throw away flags
                fs.ReadByte();

                // Extended Header Must Read Here

                ReadFrames(fs, fs.ReadSize());
                fs.Close();
                string tempPath = System.IO.Path.GetFileNameWithoutExtension(path);
                if (title == "" || title == null)
                    title = tempPath;
                if (artist == "" || artist == null)
                    artist = "Unknown";
                if (album == "" || album == null)
                    album = "Unknown";
            }
        }

        ~ID3v2()
        {
            Dispose(true);
        }

        #endregion

        #region Private Mehods

        /// <summary>
        /// Read all frames from specific FileStream
        /// </summary>
        /// <param name="Data">FileStream to read data from</param>
        /// <param name="length">Length of data to read from FileStream</param>
        private void ReadFrames(ID3TagFileStream dataStream, int length)
        {
            string frameID;
            int frameLength;
            byte buffer;
            int frameIDLength = version.Minor == 2 ? 3 : 4;

            // Minimum frame size is 10 because frame header is 10 byte
            while (length > 10 && !isComplete)
            {
                // check for padding( 00 bytes )
                buffer = dataStream.ReadByte();
                if (buffer == 0)
                {
                    length--;
                    continue;
                }

                // if read byte is not zero. it must read as FrameID
                dataStream.Seek(-1, SeekOrigin.Current);

                // ---------- Read Frame Header -----------------------
                frameID = dataStream.ReadText(frameIDLength);
                if (frameIDLength == 3 && frameIDs.Contains(frameID))
                    frameID = (string)frameIDs[frameID];

                byte[] b = new byte[frameIDLength];
                dataStream.Read(b, 0, b.Length);
                frameLength = (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];

                // Throw away 2 flag bytes
                for (int i = 0; i < 2; i++)
                    dataStream.ReadByte();
                
                long pos = dataStream.Position;

                if (length > 0x10000000)
                    throw (new Exception("This file contain frame that have more than 256MB data"));

                if (frames.Contains(frameID))
                {
                    // Throw away encoding byte
                    dataStream.ReadByte();
                    frameLength--;

                    string read = dataStream.ReadText(frameLength);
                    switch (frameID)
                    {
                        case "TPE1":
                            artist = read;
                            break;
                        case "TALB":
                            album = read;
                            break;
                        case "TIT2":
                            title = read;
                            break;
                        default:
                            break;
                    }
                    if (artist != null && album != null && title != null)
                        isComplete = true;
                }
                else
                    dataStream.Position = pos + frameLength;

                length -= frameLength + 10;
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return path + "|" + artist + "|" + album + "|" + title;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.path = null;
                    this.title = null;
                    this.artist = null;
                    this.album = null;
                    this.version = null;
                    this.frames = null;
                    this.frameIDs = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}
