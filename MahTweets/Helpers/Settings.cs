using MahTweets2.Library;
using MahTweets2.Library.Helpers;
using MahTweets2.ViewModels;
using StructureMap;

namespace MahTweets2
{
    public static class Settings
    {
        public static IAccountSettingsProvider Accounts { get; set; }
        public static IApplicationSettingsProvider Application { get; set; }

        static Settings()
        {
            Accounts = ObjectFactory.GetInstance<IAccountSettingsProvider>();
            Application = ObjectFactory.GetInstance<IApplicationSettingsProvider>();
        }

        public static void SaveSettings(MainViewModel viewModel)
        {
            // clear whatever is cached currently
            Accounts.Reset();

            foreach (var c in viewModel.ActiveMicroblogs)
            {
                Accounts.MicroblogCredentials.Add(
                    new Credential()
                    {
                        UserID = c.Credentials.UserID,
                        AccountName = c.Credentials.AccountName,
                        Username = c.Credentials.Username,
                        Password = c.Credentials.Password,
                        Protocol = c.Credentials.Protocol,
                        CustomSettings = c.Credentials.CustomSettings
                    });
            }

            foreach (var u in viewModel.ActiveUploaders)
            {
                if (!u.IMicroblogProvided)
                    Accounts.UploaderCredentials.Add(
                        new Credential()
                        {
                            UserID = u.Credentials.UserID,
                            AccountName = u.Credentials.AccountName,
                            Username = u.Credentials.Username,
                            Password = u.Credentials.Password,
                            Protocol = u.Credentials.Protocol,
                            CustomSettings = u.Credentials.CustomSettings
                        });
            }


            foreach (var sh in viewModel.StatusHandlersCollection)
            {
                if (sh.Credentials != null)
                {
                    Accounts.StatusHandlerCredentials.Add(
                        new Credential()
                        {
                            UserID = sh.Credentials.UserID,
                            AccountName = sh.Credentials.AccountName,
                            Username = sh.Credentials.Username,
                            Password = sh.Credentials.Password,
                            Protocol = sh.Credentials.Protocol,
                            CustomSettings = sh.Credentials.CustomSettings
                        });
                }
            }

            foreach (var f in viewModel.FilterGroups)
            {
                if (f.FilterList.Count != 0 || f.StreamList.Count != 0 || f.IsUnfiltered == true)
                    Accounts.Filters.Add(f);
            }

            foreach (var i in PluginEventHandler.Handlers)
            {
                Accounts.ActiveStatusHandlers.Add(i.Name);
            }

            try
            {
                var pos = GlobalPosition.GetLocation();
                if (pos.Latitude != 0)
                {
                    Application.UseLocation = true;
                    Application.Latitude = pos.Latitude;
                    Application.Longitude = pos.Longitude;
                }
                else
                    Application.UseLocation = false;
            }
            catch
            {
                Application.UseLocation = false;
            }

            Application.Save();
            Accounts.Save();
        }
    }
}
