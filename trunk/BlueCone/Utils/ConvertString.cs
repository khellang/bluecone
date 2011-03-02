using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave V�ren 2011
//      Av Terje Knutsen, Stein Arild H�iland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils
{
    public static class ConvertString
    {
        public static bool ToBoolean(string boolean)
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
    }
}
