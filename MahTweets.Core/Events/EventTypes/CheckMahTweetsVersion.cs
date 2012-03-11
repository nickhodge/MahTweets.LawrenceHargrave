using System;

namespace MahTweets.Core.Events.EventTypes
{
    public class CheckMahTweetsVersion : CompositePresentationEvent<CheckMahTweetsVersionMessage>
    {
    }

    public class CheckMahTweetsVersionMessage
    {
        public Tuple<int,int,int,string> RunningVersion { get; private set; }

        public CheckMahTweetsVersionMessage(int runningmajor, int runningminor, int runningpatch, string runningbuild)
        {
            RunningVersion = new Tuple<int, int, int, string>(runningmajor,runningminor,runningpatch,runningbuild);
        }

        public static CheckMahTweetsVersionMessage Of(int runningmajor, int runningminor, int runningpatch, string runningbuild)
        {
            return new CheckMahTweetsVersionMessage(runningmajor, runningminor, runningpatch, runningbuild);
        }
    }
}