using System;
using Microsoft.SPOT;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave V�ren 2011
//      Av Terje Knutsen, Stein Arild H�iland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone.Utils.ID3
{
    /// <summary>
    /// Interface for ID3Tags
    /// </summary>
    public interface ID3Tag : IDisposable
    {
        string Path { get; }
        string Title { get; }
        string Artist { get; }
        string Album { get; }
        bool IsComplete { get; }
    }
}
