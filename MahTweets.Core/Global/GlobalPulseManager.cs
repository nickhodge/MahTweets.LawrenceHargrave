namespace MahTweets.Core.Global
{
    public class GlobalPulseManager : WeakEventManagerBase<GlobalPulseManager, IGlobalPulseEvent>
    {
        public static GlobalPulse Pulsor = new GlobalPulse();

        protected override void StartListeningTo(IGlobalPulseEvent source)
        {
            source.Pulse += DeliverEvent;
        }

        protected override void StopListeningTo(IGlobalPulseEvent source)
        {
            source.Pulse -= DeliverEvent;
        }

        internal new void ScheduleCleanup()
        {
            base.ScheduleCleanup();
        }
    }
}