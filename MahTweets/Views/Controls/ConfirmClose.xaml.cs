using System;
using System.Windows;

namespace MahTweets.Views.Controls
{
    public partial class ConfirmClose
    {
        public ConfirmClose()
        {
            InitializeComponent();
        }

        public event EventHandler<ConfirmEventArgs> Result;

        private void OKClick(object sender, RoutedEventArgs e)
        {
            if (Result != null)
                Result(this, new ConfirmEventArgs {Result = true});
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            if (Result != null)
                Result(this, new ConfirmEventArgs {Result = false});
        }
    }

    public class ConfirmEventArgs : EventArgs
    {
        public bool Result { get; set; }
    }
}