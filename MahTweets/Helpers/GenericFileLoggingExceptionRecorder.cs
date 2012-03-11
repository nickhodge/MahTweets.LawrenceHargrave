using System;
using MahTweets.Core;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Helpers
{
    public class GenericFileLoggingExceptionRecorder : IExceptionReporter
    {
        #region IExceptionReporter Members

        public void ReportHandledException(Exception ex)
        {
#if DEBUG
            BlackBoxRecorder.LegacyLogHandledException(ex);
#endif
        }

        public void ReportCrash(Exception ex, string UserEmail, string UserReport)
        {
            ReportHandledException(ex);
        }

        #endregion
    }
}