using System;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IExceptionReporter
    {
        void ReportHandledException(Exception ex);
        void ReportCrash(Exception ex, string UserEmail, string UserReport);
    }
}