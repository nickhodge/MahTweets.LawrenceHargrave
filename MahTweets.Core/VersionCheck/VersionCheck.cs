using System;
using MahTweets.Core.Composition;
using MahTweets.Core.Events;
using MahTweets.Core.Events.EventTypes;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using Newtonsoft.Json.Linq;

namespace MahTweets.Core.VersionCheck
{
    public class CurrentVersion : ICurrentVersionCheck
    {
        // get version, compare
        // if no changes, no notification
        // else create notification new version with message

        private readonly IApplicationSettingsProvider _applicationSettingsProvider;
        private readonly IEventAggregator _eventAggregator;

        private const string Versioncheckurl = "http://service.lawrencehargrave.com/1/versioncheck/release";
 
        public CurrentVersion(            
            IApplicationSettingsProvider applicationSettingsProvider,
            IEventAggregator eventAggregator
            )
        {
            _eventAggregator = eventAggregator;
            _applicationSettingsProvider = applicationSettingsProvider;
            _eventAggregator.GetEvent<CheckMahTweetsVersion>().Subscribe(HandleCheckMahTweetsVersion);
        }

        public async void HandleCheckMahTweetsVersion (CheckMahTweetsVersionMessage currentVersion)
        {
            var webFetcher = new AsyncWebFetcher();
            try
            {
                var versionraw = await webFetcher.FetchAsync(Versioncheckurl);
                var j = JObject.Parse(versionraw);

                var currentmajor = j["major"].ToObject<Int32>();
                var currentminor = j["minor"].ToObject<Int32>();
                var currentpatch = j["patch"].ToObject<Int32>();
                var currentbuild = j["build"].ToObject<String>();

                var runningmajor = currentVersion.RunningVersion.Item1;
                var runningminor = currentVersion.RunningVersion.Item2;
                var runningpatch = currentVersion.RunningVersion.Item3;
                var runningbuild = currentVersion.RunningVersion.Item4;

                var flagupdate = currentmajor > runningmajor || currentminor > runningminor || currentpatch > runningpatch || currentbuild.ToLower() != runningbuild.ToLower();

                if (!flagupdate) return;

                var infourl = j["infourl"].ToObject<String>();
                var downloadurl = j["downloadurl"].ToObject<String>();
                var messagetext = j["messagetext"].ToObject<String>();
                var versiontext = string.Format("{0}.{1}.{2}.{3}", currentmajor, currentminor, currentpatch,
                                                currentbuild);

                _eventAggregator.GetEvent<ShowNotification>().Publish(
                    new ShowNotificationPayload(new UpdateNotification(messagetext, versiontext, downloadurl, infourl),
                                                TimeSpan.FromMinutes(1), NotificactionLevel.Information));
            } catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
        }
    }
}
