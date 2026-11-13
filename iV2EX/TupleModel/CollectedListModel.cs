using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace iV2EX.TupleModel
{
    public class CollectedListModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Text { get; set; }
        public string Name { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
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