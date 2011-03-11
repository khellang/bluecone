using System;
using System.IO;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    /// <summary>
    /// Partial ID3v1 Frame.
    /// </summary>
    public class ID3v1 : ID3Tag, IDisposable
    {
        #region Fields

        private bool disposed = false;

        private string path;
        private string title;
        private string artist;
        private string album;
        private bool isComplete;

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

        #endregion

        #region Ctor

        public ID3v1(string path)
        {
            this.path = path;
            using (ID3TagFileStream fs = new ID3TagFileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (!fs.HaveID3v1())
                {
                    fs.Close();
                    isComplete = false;
                    return;
                }
                title = fs.ReadText(30);
                fs.Seek(-95, SeekOrigin.End);
                artist = fs.ReadText(30);
                fs.Seek(-65, SeekOrigin.End);
                album = fs.ReadText(30);
                fs.Close();
                isComplete = true;
            }
        }

        ~ID3v1()
        {
            Dispose(true);
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
                }
                disposed = true;
            }
        }

        #endregion
    }
}
