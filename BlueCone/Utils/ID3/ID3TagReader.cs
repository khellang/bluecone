using System;
using Microsoft.SPOT;
using System.IO;
using System.Text;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    /// <summary>
    /// Utility class for reading Id3Tags :)
    /// </summary>
    public static class ID3TagReader
    {
        #region Static Methods

        /// <summary>
        /// Method for reading the the ID3Tag of an mp3-file.
        /// </summary>
        /// <param name="path">The path of the mp3-file.</param>
        /// <returns>String array with path, artist, album and title</returns>
        public static ID3Tag ReadFile(string path)
        {
            Debug.Print("Reading ID3Tag from " + path);
            ID3Tag tag = new ID3v1(path);
            if (tag.IsComplete)
                return tag;
            else
            {
                tag = new ID3v2(path);
                if (tag.IsComplete)
                    return tag;
                else
                    return new UnknownID3Tag(path);
            }
        }

        #endregion
    }
}
