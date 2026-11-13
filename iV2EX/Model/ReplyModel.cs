using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iV2EX.Model
{
    public class ReplyModel : INotifyPropertyChanged
    {
        private int _thanks;
        private bool _isThanked;

        public string ReplyDate { get; set; }

        public bool IsLz { get; set; }

        public int Id { get; set; }

        public int Thanks
        {
            get => _thanks;
            set
            {
                if (_thanks == value) return;
                _thanks = value;
                OnPropertyChanged();
            }
        }

        public string Content { get; set; }

        public string Avater { get; set; }

        public string Username { get; set; }

        public string Token { get; set; }

        public string Floor { get; set; }

        public bool IsThanked
        {
            get => _isThanked;
            set
            {
                if (_isThanked == value) return;
                _isThanked = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}