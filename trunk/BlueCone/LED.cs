using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

//-----------------------------------------------------------------------
//  BlueCone Bacheloroppgave Våren 2011
//      Av Terje Knutsen, Stein Arild Høiland og Kristian Hellang
//-----------------------------------------------------------------------

namespace BlueCone
{
    public static class LED
    {
        #region Fields

        private static OutputPort ledPort;
        private static LEDState state;
        private static Thread ledThread;

        #endregion

        #region Properties

        public static LEDState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        #endregion

        #region Methods

        public static void Initialize()
        {
            ledPort = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di10, true);
            state = LEDState.Initializing;
            ledThread = new Thread(ControlLED);
            ledThread.Priority = ThreadPriority.BelowNormal;
            ledThread.Start();
        }

        private static void ControlLED()
        {
            while (true)
            {
                switch (state)
                {
                    case LEDState.Initializing:
                        ledPort.Write(true);
                        Thread.Sleep(1000);
                        ledPort.Write(false);
                        Thread.Sleep(1000);
                        break;
                    case LEDState.Ready:
                        if (!ledPort.Read())
                        {
                            ledPort.Write(true);
                        }
                        Thread.Sleep(50);
                        break;
                    case LEDState.Playing:
                        ledPort.Write(true);
                        Thread.Sleep(500);
                        ledPort.Write(false);
                        Thread.Sleep(500);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }

    public enum LEDState
    {
        Initializing,
        Ready,
        Playing
    }
}
