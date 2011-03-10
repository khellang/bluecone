using System;
using Microsoft.SPOT;
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
    public class ID3v1 : ID3Tag
    {
        #region Fields

        private string path;
        private string title;
        private string artist;
        private string album;
        private bool hasTag;

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

        public bool HasTag
        {
            get { return this.hasTag; }
        }

        #endregion

        #region Ctor

        public ID3v1(string path)
        {
            this.path = path;
            using (ID3TagStream fs = new ID3TagStream(path, FileMode.Open))
            {
                if (!fs.HaveID3v1())
                {
                    fs.Close();
                    hasTag = false;
                    return;
                }
                title = fs.ReadText(30);
                fs.Seek(-95, SeekOrigin.End);
                artist = fs.ReadText(30);
                fs.Seek(-65, SeekOrigin.End);
                album = fs.ReadText(30);
                fs.Close();
                hasTag = true;
            }
        }

        #endregion
    }
}
