using System.Windows.Input;
using MahTweets.Core.Interfaces.ViewModels;

namespace MahTweets.Core.ViewModels
{
    public class ContainerViewModel : BaseViewModel, IContainerViewModel
    {
        private ICommand _closeCommand;
        private int _position;
        private string _title;
        private string _uuid;
        private double _width;

        public string Uuid
        {
            get
            {
                if (_uuid == null)
                    _uuid = Composition.Uuid.NewUuid();
                return _uuid;
            }
            set { _uuid = value; }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public double Width
        {
            get
            {
                if (_width == 0)
                    _width = 310;

                return _width;
            }
            set
            {
                _width = value;

                RaisePropertyChanged(() => Width);
            }
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
            set
            {
                _closeCommand = value;
                RaisePropertyChanged(() => CloseCommand);
            }
        }

        #region IContainerViewModel Members

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RaisePropertyChanged(() => Position);
            }
        }

        #endregion

        public virtual void Start()
        {
        }

        public virtual void Close()
        {
        }

        public virtual void RemoveContainer(ContainerViewModel viewModel)
        {
        }
    }
}