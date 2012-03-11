using System.Linq;
using System.Windows;
using MahTweets.ViewModels;

namespace MahTweets.Views
{
    public partial class SetupView
    {
        private readonly SetupViewModel _model;

        public SetupView(SetupViewModel model)
        {
            InitializeComponent();

            Unloaded += SetupViewUnloaded;

            _model = model;
            _model.View = this;
            DataContext = model;

            if (model.PluginRepository.Microblogs.Any())
                VisualStateManager.GoToState(this, "ShowPlugins", true);
            else
                VisualStateManager.GoToState(this, "ShowStartup", true);
        }

        private void SetupViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                _model.GlobalIgnoresViewModel.CheckForNewUpdates();
            }
        }
    }
}