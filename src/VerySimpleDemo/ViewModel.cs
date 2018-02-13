using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfAsyncRequest;

namespace VerySimpleDemo
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region AskUserRequest

        public IAsyncRequest<string, bool?> AskUserRequest
            => _askUserRequest ?? (_askUserRequest = AsyncRequest.CreateSingleResponseRequest<string, bool?>());

        private IAsyncRequest<string, bool?> _askUserRequest;

        #endregion

        #region AskUserCommand

        public ICommand AskUserCommand => _askUserCommand ?? (_askUserCommand = new AsyncCommand(AskUserAsync));

        private ICommand _askUserCommand;

        private async Task AskUserAsync()
        {
            // Send request to view and wait for response
            var result = await AskUserRequest.SendAsync("Do you?");

            switch (result)
            {
                case null:
                    State = "The user is shy!";
                    break;
                case true:
                    State = "The user does!";
                    break;
                default:
                    State = "The user doesn't!";
                    break;
            }
        }

        #endregion

        #region State

        public string State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        private string _state;

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
