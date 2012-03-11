using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MahTweets.Core.Global
{
    public class GlobalPulse : IGlobalPulseEvent
    {
        private readonly Timer _pulseTimer;

        public GlobalPulse()
        {
            _pulseTimer = new Timer(20000);
            Start();
        }

        #region IGlobalPulseEvent Members

        public event PulseEventHandler Pulse;

        #endregion

        public void Start()
        {
            _pulseTimer.Elapsed -= PulseTimerElapsed;
            _pulseTimer.Stop();
            _pulseTimer.Elapsed += PulseTimerElapsed;
            _pulseTimer.Start();
        }

        public void Stop()
        {
            _pulseTimer.Elapsed -= PulseTimerElapsed;
            _pulseTimer.Stop();
        }

        private void PulseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SendPulse();
            GlobalPulseManager.Current.ScheduleCleanup();
        }

        private void SendPulse()
        {
            var sw = new Stopwatch();
            if (Pulse != null)
            {
                sw.Start();
                Pulse(this, new EventArgs());
                sw.Stop();
                // Logging.Info("Pulse took {0} ms", sw.ElapsedMilliseconds.ToString());
            }

            // Provisional hack to see if we can force Generation-2 items to be cleared. 
            ThreadPool.QueueUserWorkItem(state =>
                                             {
                                                 sw.Reset();
                                                 sw.Start();
                                                 GC.Collect(2, GCCollectionMode.Forced);
                                                 sw.Stop();
                                                 // Logging.Info("Forced Gen-2 Garbage Collection took {0} ms", sw.ElapsedMilliseconds.ToString());
                                             });
        }
    }
}