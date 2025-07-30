using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ViewModel.Contact.ContactDataVM
{
    public partial class ContactDataGenericSingletonVM<T> : ObservableObject, IGenericPersonDataVM where T : new()
    {
        private string _backUpValue;

        private string _fieldMask;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEnabled))]
        private bool _isDataReadOnly;

        [ObservableProperty]
        private T _value;

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
                return _backUpValue != JsonConvert.SerializeObject(Value);
            }
        }

        public bool IsEnabled { get => !IsDataReadOnly; }
        public ContactDataGenericSingletonVM(string fieldMask)
        {
            _fieldMask = fieldMask;
        }

        public void SetNewValue(T? value)
        {
            Value = new();

            if (value is null)
            {
                _backUpValue = JsonConvert.SerializeObject(Value);
                return;
            }

            Value = value;
            _backUpValue = JsonConvert.SerializeObject(value);
        }
    }
}