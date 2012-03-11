using System.Windows.Input;

namespace MahTweets.Views.Setup
{
    public partial class GlobalIgnoresView
    {
        public GlobalIgnoresView()
        {
            InitializeComponent();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            // TODO: either revert to this behaviour or use some commanding
            //if (e.Key == Key.Enter)
            //{
            //    _applicationSettings.GlobalExclude.Add(txtFilter.Text);
            //    txtFilter.Text = String.Empty;
            //}
        }
    }
}