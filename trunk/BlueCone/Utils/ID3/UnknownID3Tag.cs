using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    public class UnknownID3Tag : ID3Tag, IDisposable
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

        public UnknownID3Tag(string path)
        {
            this.path = path;
            this.title = path;
            this.artist = "Unknown";
            this.album = "Unknown";
            this.isComplete = true;
        }

        ~UnknownID3Tag()
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
