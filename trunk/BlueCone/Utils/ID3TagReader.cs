using System;
using Microsoft.SPOT;
using System.IO;
using System.Text;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
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
        public static string[] ReadFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                fs.Seek(-125, SeekOrigin.End);
                byte[] buffer = new byte[30]; // Kanskje det er best å ha denne static?
                fs.Read(buffer, 0, 30);
                string title = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                fs.Read(buffer, 0, 30);
                string artist = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                fs.Read(buffer, 0, 30);
                string album = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                fs.Close();
                return new string[] { path, artist, album, title };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}
