using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public static class Converting
    {
        public static bool StringToBoolean(string boolean)
        {
            switch (boolean)
            {
                case "True":
                    return true;
                case "False":
                    return false;
                case "true":
                    return true;
                case "false":
                    return false;
                case "TRUE":
                    return true;
                default:
                    return false;
            }
        }

        public static string ByteToHex(byte b)
        {
            const string hex = "0123456789ABCDEF";
            int lowNibble = b & 0x0F;
            int highNibble = (b & 0xF0) >> 4;
            string s = new string(new char[] { hex[highNibble], hex[lowNibble] });
            return s;
        }
    }
}
