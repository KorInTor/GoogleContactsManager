using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.PeopleService.v1.Data;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using ViewModel.Contact.ContactDataVM;
using ViewModel.Contact.ValuePicker;
using ViewModel.Extensions;

namespace ViewModel.Contact
{
    public partial class SingleContactVM : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEnabled))]
        public bool _isDataReadOnly;

        private static readonly HttpClient _httpClient = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsVisible))]
        private Person? _currentPerson;

        [ObservableProperty]
        private EditeState _editeState;

        private readonly ContactDataGenericVM<EmailAddress> _emailAddressVM = new("emailAddresses");
        private readonly ContactDataGenericVM<PhoneNumber> _phoneNumbersVM = new("phoneNumbers");
        private readonly ContactDataGenericSingletonVM<Name> _nameVM = new("names");
        private readonly ContactDataGenericSingletonVM<GoogleDatePickerVM> _birthdayVM = new("birthdays");
        private readonly ContactGroupPopupVM _groupPopupVM = new();

        private readonly List<IGenericPersonDataVM> _genericPersonDataVMs = [];

        [ObservableProperty]
        private BitmapImage? _personImage;

        public IRelayCommand CancelChanges { get; set; }
        public IRelayCommand EditCommand { get; set; }
        public bool IsEnabled { get => !IsDataReadOnly; }
        public bool IsVisible { get => CurrentPerson != null; }
        public ContactDataGenericSingletonVM<Name> NameVM => _nameVM;
        public ContactDataGenericVM<EmailAddress> EmailAddressVM => _emailAddressVM;
        public ContactDataGenericVM<PhoneNumber> PhoneNumbersVM => _phoneNumbersVM;
        public ContactDataGenericSingletonVM<GoogleDatePickerVM> BirthdayVM => _birthdayVM;
        public ContactGroupPopupVM GroupPopupVM => _groupPopupVM;
        public IRelayCommand SaveChanges { get; set; }

        public SingleContactVM(IRelayCommand editCommand, IRelayCommand saveChanges, IRelayCommand cancelChanges, IList<ContactGroup> validContactGroups)
        {
            EditCommand = editCommand;
            SaveChanges = saveChanges;
            CancelChanges = cancelChanges;
            FillGenericVMList();
            _groupPopupVM.PossibleValues = validContactGroups;
        }

        public IEnumerable<string> GetChangedPersonFields()
        {
            return _genericPersonDataVMs
                .Where(vm => vm.IsDataChanged)
                .Select(vm => vm.FieldMask)
                .ToList();
        }

        public void UpdateCurrentPersonDataFromUI()
        {
            if (CurrentPerson is null)
                return;
            CurrentPerson.PhoneNumbers = PhoneNumbersVM.Values;
            CurrentPerson.Names = [NameVM.Value];
            CurrentPerson.EmailAddresses = EmailAddressVM.Values;
            CurrentPerson.Birthdays = BirthdayVM.IsDataChanged ? [BirthdayVM.Value.ToBirthday()] : null;
            CurrentPerson.Memberships = GroupPopupVM.ActualValues;
        }

        public void UpdateUIDataFromCurrentPerson()
        {
            if (CurrentPerson is null)
                return;

            PhoneNumbersVM.SetNewValue(CurrentPerson.PhoneNumbers);
            NameVM.SetNewValue(CurrentPerson.Names?.FirstOrDefault());
            EmailAddressVM.SetNewValue(CurrentPerson.EmailAddresses);
            BirthdayVM.SetNewValue(new(CurrentPerson.Birthdays?.FirstOrDefault()?.Date));
            GroupPopupVM.ActualValues = CurrentPerson.Memberships;
        }

        private void FillGenericVMList()
        {
            _genericPersonDataVMs.Add(PhoneNumbersVM);
            _genericPersonDataVMs.Add(NameVM);
            _genericPersonDataVMs.Add(EmailAddressVM);
            _genericPersonDataVMs.Add(BirthdayVM);
            _genericPersonDataVMs.Add(GroupPopupVM);
        }

        private async Task LoadAndSetImageAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                PersonImage = null;
                return;
            }

            try
            {
                var imageData = await _httpClient.GetByteArrayAsync(url);

                await using var stream = new MemoryStream(imageData);
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                Log.Information("Image Downloaded successfully");
                PersonImage = bitmap;
            }
            catch (Exception ex)
            {
                Log.Error($"Ошибка загрузки изображения: {ex.Message}");
                PersonImage = null;
            }
        }

        partial void OnCurrentPersonChanged(Person? value)
        {
            IsDataReadOnly = true;
            if (value == null)
                return;

            _ = LoadAndSetImageAsync(value.Photos?.FirstOrDefault()?.Url);

            UpdateUIDataFromCurrentPerson();
        }

        partial void OnIsDataReadOnlyChanged(bool value)
        {
            _genericPersonDataVMs.ForEach(genericVm => { genericVm.IsDataReadOnly = value; });
        }
    }
}
