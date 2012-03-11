namespace MahTweets.Core.Global
{
    public interface IGlobalPulseEvent
    {
        event PulseEventHandler Pulse;
    }
}