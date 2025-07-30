using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ViewModel.Contact.ContactDataVM
{
    public partial class ContactDataGenericVM<T> : ObservableObject, IGenericPersonDataVM where T : new()
    {
        private string _backUpValue;

        private string _fieldMask;

        public string FieldMask
        {
            get
            {
                return _fieldMask;
            }
        }

        public bool IsDataChanged
        {
            get
            {
                return _backUpValue != JsonConvert.SerializeObject(Values.ToList());
            }
        }

        public bool IsEnabled { get => !IsDataReadOnly; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEnabled))]
        [NotifyCanExecuteChangedFor(nameof(AddValueCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteValueCommand))]
        private bool _isDataReadOnly;

        [ObservableProperty]
        ObservableCollection<T> _values = [];

        [ObservableProperty]
        T _selectedValue;

        public RelayCommand AddValueCommand { get; set; }

        public RelayCommand<T> DeleteValueCommand { get; set; }

        public ContactDataGenericVM(string fieldMask)
        {
            _fieldMask = fieldMask;
            AddValueCommand = new RelayCommand(AddValue, () => IsEnabled);
            DeleteValueCommand = new RelayCommand<T>(DeleteValue, (T) => IsEnabled);
        }

        private void DeleteValue(T? deletedValue)
        {
            if (deletedValue is null)
                return;
            Values.Remove(deletedValue);
        }

        public void SetNewValue(IList<T>? values)
        {
            Values.Clear();
            if (values is null)
            {
                _backUpValue = JsonConvert.SerializeObject(Values);
                return;
            }

            foreach (var newValue in values)
            {
                Values.Add(newValue);
            }

            _backUpValue = JsonConvert.SerializeObject(values.ToList());
        }

        private void AddValue()
        {
            Values ??= [];
            Values.Add(new());
        }
    }
}
