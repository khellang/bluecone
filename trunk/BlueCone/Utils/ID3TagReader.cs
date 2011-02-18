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
        private static FileStream fs;

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
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                fs.Seek(-128, SeekOrigin.End);
                byte[] buffer = new byte[30]; // Kanskje det er best å ha denne static?
                fs.Read(buffer, 0, 3);
                string TAG = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                if (TAG == "TAG")
                {
                    fs.Read(buffer, 0, 30);
                    string title = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                    fs.Read(buffer, 0, 30);
                    string artist = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                    fs.Read(buffer, 0, 30);
                    string album = new string(Encoding.UTF8.GetChars(buffer)).Trim();
                    return new string[] { path, artist, album, title };
                }
                else
                    throw new InvalidOperationException("File \"" + path + "\" does not contain and ID3Tag!");
            }
            catch (Exception)
            {
                Debug.Print("Unable to read ID3Tag from " + path);
                return new string[] { path, "Ukjent", "Ukjent", path };
            }
            finally
            {
                fs.Close();
            }
        }

        #endregion
    }
}
