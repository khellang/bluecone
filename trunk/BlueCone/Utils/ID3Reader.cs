using System;
using Microsoft.SPOT;
using System.IO;
using System.Text;

namespace BlueCone.Utils
{
    public static class ID3Reader
    {
        public static string[] ReadFile(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                fs.Seek(-125, SeekOrigin.End);
                byte[] buffer = new byte[30];
                fs.Read(buffer, 0, 30);
                string title = new string(Encoding.UTF8.GetChars(buffer));
                fs.Read(buffer, 0, 30);
                string artist = new string(Encoding.UTF8.GetChars(buffer));
                fs.Read(buffer, 0, 30);
                string album = new string(Encoding.UTF8.GetChars(buffer));
                fs.Close();
                return new string[] { artist, title, album };
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
