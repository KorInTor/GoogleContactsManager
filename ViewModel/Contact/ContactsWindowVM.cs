using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using Serilog;
using System.ComponentModel;
using System.Net;

namespace ViewModel.Contact
{
    public partial class ContactsWindowVM : ObservableObject
    {
        private readonly ContactSelectorVM _contactSelectorVM;
        private readonly SingleContactVM _singleContactVM;
        private readonly ContactGroupVM _contactGroupSelectorVM;

        private PeopleServiceService? _service = null;
        public PeopleServiceService Service 
        {
            get
            {
                if (_service is null)
                    throw new NullReferenceException("People Service was not initialized");
                return _service;
            }
        }

        [ObservableProperty]
        private UserCredential? _userCredential;

        private readonly string[] _validFields =
        [
            "addresses", "ageRanges", "biographies", "birthdays", "calendarUrls", "clientData",
            "coverPhotos", "emailAddresses", "events", "externalIds", "genders", "imClients",
            "interests", "locales", "locations", "memberships", "metadata", "miscKeywords",
            "names", "nicknames", "occupations", "organizations", "phoneNumbers", "photos",
            "relations", "sipAddresses", "skills", "urls", "userDefined"
        ];

        private readonly string[] _unsuportedGroupNames = ["friends", "family", "coworkers", "chatBuddies", "blocked"];

        public string ValidFieldsCombined { get => string.Join(",", _validFields); }

        public ContactSelectorVM ContactSelectorVM { get => _contactSelectorVM; }
        public SingleContactVM SingleContactVM { get => _singleContactVM; }
        public ContactGroupVM ContactGroupVM { get => _contactGroupSelectorVM; }

        public string JsonBackUp { get; private set; } = "";

        public List<Person> Contacts { get; } = [];

        public ContactsWindowVM()
        {
            _contactGroupSelectorVM = new ContactGroupVM(AddGroupCommand, EditGroupCommand);
            _contactSelectorVM = new ContactSelectorVM(EditContactCommand, AddContactCommand, DeleteContactCommand, Contacts);
            _singleContactVM = new SingleContactVM(EditContactCommand, SaveChangesCommand, CancelChangesCommand, ContactGroupVM.ContactGroups);
            _contactGroupSelectorVM.DeleteDataRequest += DeleteGroupHandler;
            
            ContactSelectorVM.PropertyChanged += ContactSelectorVM_PropertyChanged;
            ContactGroupVM.PropertyChanged += ContactGroupVM_PropertyChanged;
        }

        [RelayCommand]
        private void AddGroup()
        {
            ContactGroupVM.EditeState = EditeState.Create;
            string? newName = TextInputRequested?.Invoke("Создание группы", "Введите название новой группы контактов");
            if (newName is null)
                return;

            var createRequestBody = new CreateContactGroupRequest { ContactGroup = new() { Name = newName } };
            var createRequest = Service.ContactGroups.Create(createRequestBody);
            ExecuteRequestSafe(createRequest.Execute);
            ContactGroupVM.EditeState = EditeState.None;
            UpdateContactGroups();
        }

        [RelayCommand]
        private void EditGroup(ContactGroup? group)
        {
            if (group is null)
                return;

            ContactGroupVM.EditeState = EditeState.Update;
            string? newName = TextChangeRequested?.Invoke("Изменение группы", "Введите новое название группы контактов", group.Name);
            if (newName is null)
                return;

            group.Name = newName;

            var updateRequest = new UpdateContactGroupRequest
            {
                ContactGroup = group
            };
            var resourceUpdateRequest = Service.ContactGroups.Update(updateRequest, group.ResourceName);
            var newContact = ExecuteRequestSafe(resourceUpdateRequest.Execute);
            if (newContact is null)
                return;

            for (int i = 0; i < ContactGroupVM.ContactGroups.Count; i++)
            {
                if (ContactGroupVM.ContactGroups[i].ResourceName == newContact.ResourceName)
                {
                    ContactGroupVM.ContactGroups[i] = newContact;
                    break;
                }
            }

            ContactGroupVM.EditeState = EditeState.None;
        }

        private T? ExecuteRequestSafe<T>(Func<T> executeFunction, string? userErrorMessage = null)
        {
            T? returnValue = default;
            try
            {
                returnValue = executeFunction.Invoke();
            }
            catch (Exception ex)
            {
                OnExceptionNotify(ex, userErrorMessage);
            }
            return returnValue;
        }

        private bool DeleteGroupHandler(ContactGroup? groupToDelete)
        {
            if (ConfirmRequested is null || groupToDelete is null)
                return false;

            bool confirmed = ConfirmRequested.Invoke("Удалить элемент", "Удалить данные о контактах группы?");
            var delReq = Service.ContactGroups.Delete(groupToDelete.ResourceName);
            delReq.DeleteContacts = confirmed;
            if (ExecuteRequestSafe(delReq.Execute) is not null)
            {
                ContactGroupVM.EditeState = EditeState.None;
                return true;
            }
            return false;
        }

        [RelayCommand]
        private void AddContact()
        {
            if (ContactGroupVM.SelectedGroup is null)
                throw new ArgumentNullException(nameof(ContactGroupVM.SelectedGroup));

            Person newPerson = new();
            Contacts.Add(newPerson);
            newPerson.Memberships = [];
            newPerson.Memberships.Add(new Membership());
            newPerson.Memberships[0].ContactGroupMembership = new();
            if (ContactGroupVM.SelectedGroup.ResourceName == "contactGroups/all")
            {
                newPerson.Memberships[0].ContactGroupMembership.ContactGroupResourceName = "contactGroups/myContacts";
            }
            else
            {
                newPerson.Memberships[0].ContactGroupMembership.ContactGroupResourceName = ContactGroupVM.SelectedGroup.ResourceName;
            }
            ContactSelectorVM.SelectedPerson = newPerson;
            SingleContactVM.EditeState = EditeState.Create;
            SingleContactVM.IsDataReadOnly = false;
            ContactGroupVM.IsEnabled = false;
            ContactSelectorVM.IsEnabled = false;
        }

        [RelayCommand]
        private void CancelChanges()
        {
            if (ContactSelectorVM.SelectedPerson is null)
                return;

            var index = Contacts.IndexOf(ContactSelectorVM.SelectedPerson); ;

            if (SingleContactVM.EditeState == EditeState.Create)
            {
                if (index == 0)
                {
                    ContactSelectorVM.SelectedPersonIndex = -1;
                    Contacts.Clear();
                }
                else
                {
                    ContactSelectorVM.SelectedPersonIndex--;
                    Contacts.RemoveAt(index);
                }

                ContactGroupVM.IsEnabled = true;
                ContactSelectorVM.IsEnabled = true;
                return;
            }

            if (SingleContactVM.EditeState == EditeState.Update)
            {
                var backUpPerson = JsonConvert.DeserializeObject<Person>(JsonBackUp) ?? throw new ArgumentNullException("Failed to deserialize Backup person");
                Contacts[index] = backUpPerson;
                ContactSelectorVM.SelectedPerson = Contacts[index];
                ContactGroupVM.IsEnabled = true;
                ContactSelectorVM.IsEnabled = true;
            }
        }

        [RelayCommand]
        private void DeleteContact()
        {
            if (ContactSelectorVM.SelectedPerson is null)
                return;

            Person deletePerson = ContactSelectorVM.SelectedPerson;
            var result = ExecuteRequestSafe(Service.People.DeleteContact(deletePerson.ResourceName).Execute);
            if (result is null)
                return;

            if (ContactSelectorVM.SelectedPersonIndex == 0)
            {
                ContactSelectorVM.SelectedPersonIndex = -1;
                Contacts.Clear();
                return;
            }

            ContactSelectorVM.SelectedPersonIndex--;
            Contacts.Remove(ContactSelectorVM.SelectedPerson);
        }

        [RelayCommand]
        private void EditContact()
        {
            SingleContactVM.IsDataReadOnly = false;
            SingleContactVM.EditeState = EditeState.Update;
            JsonBackUp = JsonConvert.SerializeObject(SingleContactVM.CurrentPerson);
            ContactGroupVM.IsEnabled = false;
            ContactSelectorVM.IsEnabled = false;
        }

        partial void OnUserCredentialChanged(UserCredential? value)
        {
            if (value is null)
                throw new ArgumentNullException("User credentials cannot be null");

            InitService();
            UpdateContactGroups();
            UpdateContacts();
        }

        private void UpdateContacts()
        {
            Contacts.Clear();
            var listReq = Service.People.Connections.List("people/me");
            listReq.PersonFields = ValidFieldsCombined;
            var responceList = ExecuteRequestSafe(listReq.Execute);
            if (responceList is null || responceList.Connections is null)
                return;

            foreach (var contact in responceList.Connections)
                Contacts.Add(contact);

            ContactSelectorVM.ContactViewCollection.Refresh();
        }

        private void UpdateContactGroups()
        {
            var listGroupReq = Service.ContactGroups.List();
            var listGroupResp = ExecuteRequestSafe(listGroupReq.Execute);
            if (listGroupResp is null)
                return;

            ContactGroupVM.ContactGroups.Clear();
            foreach (var group in listGroupResp.ContactGroups)
            {
                if (_unsuportedGroupNames.Contains(group.Name))
                    continue;

                if (group.Metadata is not null)
                    if (group.Metadata.Deleted is not null)
                        if (group.Metadata.Deleted.Value)
                            continue;

                ContactGroupVM.ContactGroups.Add(group);
            }
        }

        private void InitService()
        {
            _service = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = UserCredential,
                ApplicationName = "People API Sample",
            });
        }

        public bool TryGetPeopleForGroup(ref IEnumerable<Person> personsList, string groupResoureName)
        {
            var getGroupReq = Service.ContactGroups.Get(groupResoureName);
            getGroupReq.MaxMembers = 1000;
            ContactGroup? groupResp;
            groupResp = ExecuteRequestSafe(getGroupReq.Execute);
            if (groupResp is null)
                return false;

            List<string> memberNames = [];

            if (groupResp.MemberResourceNames != null)
            {
                foreach (var memberName in groupResp.MemberResourceNames)
                {
                    memberNames.Add(memberName);
                }
            }

            if (memberNames.Count == 0)
                return true;

            var getReq = Service.People.GetBatchGet();
            getReq.ResourceNames = memberNames;
            getReq.PersonFields = ValidFieldsCombined;
            var responce = ExecuteRequestSafe(getReq.Execute);
            if (responce is not null)
                personsList = responce.Responses.Select(resp => resp.Person);
            else
                return false;

            return true;
        }

        [RelayCommand]
        private void SaveChanges()
        {
            if (ContactSelectorVM.SelectedPerson is null)
                return;

            SingleContactVM.IsDataReadOnly = true;
            SingleContactVM.UpdateCurrentPersonDataFromUI();
            string personFields = string.Join(",", SingleContactVM.GetChangedPersonFields().Select(personField => $"{personField}"));
            if (string.IsNullOrEmpty(personFields))
                return;

            Person? newPersonInfo = null;

            switch (SingleContactVM.EditeState)
            {
                case EditeState.Update:
                    _ = TryUpdateContact(personFields, ref newPersonInfo);
                    break;

                case EditeState.Create:
                    var createContactRequest = Service.People.CreateContact(SingleContactVM.CurrentPerson);
                    createContactRequest.PersonFields = personFields;
                    newPersonInfo = ExecuteRequestSafe(createContactRequest.Execute);
                    break;

                default:
                    throw new NotSupportedException("This edit state not currently supported.");
            }

            newPersonInfo ??= JsonConvert.DeserializeObject<Person>(JsonBackUp) ?? throw new ArgumentNullException("Failed to deserialize Backup person");
            int originIndex = Contacts.IndexOf(ContactSelectorVM.SelectedPerson);
            Contacts[originIndex] = newPersonInfo;
            ContactSelectorVM.SelectedPerson = Contacts[originIndex];
            JsonBackUp = string.Empty;
            ContactGroupVM.IsEnabled = true;
            ContactSelectorVM.IsEnabled = true;
        }

        private bool TryUpdateContact(string personFields, ref Person? newPersonInfo)
        {
            if (SingleContactVM.CurrentPerson is null)
                return false;

            var updateContactRequest = Service.People.UpdateContact(SingleContactVM.CurrentPerson, SingleContactVM.CurrentPerson.ResourceName);
            updateContactRequest.UpdatePersonFields = personFields;
            try
            {
                newPersonInfo = updateContactRequest.Execute();
            }
            catch (GoogleApiException updateException) when (updateException.HttpStatusCode == HttpStatusCode.PreconditionFailed)
            {
                OnExceptionNotify(updateException, "Данный контакт уже обновлен удаленно!\nПосле обновления данных попробуйте снова.");
                var getContactRequest = Service.People.Get(SingleContactVM.CurrentPerson.ResourceName);
                getContactRequest.PersonFields = ValidFieldsCombined;
                newPersonInfo = ExecuteRequestSafe(getContactRequest.Execute);
                if (newPersonInfo is not null)
                {
                    OnInformationNotify("Precognition failed handled succesfully", "Данные обновлены успешно");
                    return true;
                }

                return false;
            }
            catch (Exception updateException)
            {
                OnExceptionNotify(updateException);
                return false;
            }
            return true;
        }

        private void OnExceptionNotify(Exception ex, string? userMessage = null)
        {
            userMessage ??= ex.Message;
            Log.Error(ex.Message);
            ErrorMessage?.Invoke(userMessage);
        }

        private void OnInformationNotify(string logMessage, string? userMessage)
        {
            Log.Information(logMessage);
            if (!string.IsNullOrEmpty(userMessage))
                InformationMessage?.Invoke(userMessage);
        }

        private void ContactGroupVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ContactGroupVM.SelectedGroup))
            {
                if (ContactGroupVM.SelectedGroup is null)
                    return;

                ContactSelectorVM.TargetPerson.Memberships ??= [];
                ContactSelectorVM.TargetPerson.Memberships.Clear();
                ContactSelectorVM.TargetPerson.Memberships.Add(new Membership());
                ContactSelectorVM.TargetPerson.Memberships[0].ContactGroupMembership ??= new ContactGroupMembership();
                ContactSelectorVM.TargetPerson.Memberships[0].ContactGroupMembership.ContactGroupResourceName = ContactGroupVM.SelectedGroup.ResourceName;
                ContactSelectorVM.ContactViewCollection.Refresh();
            }
        }

        private void ContactSelectorVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ContactSelectorVM.SelectedPerson))
            {
                if (ContactSelectorVM.SelectedPerson is null)
                    return;

                SingleContactVM.CurrentPerson = ContactSelectorVM.SelectedPerson;
                SingleContactVM.EditeState = EditeState.None;
            }
        }

        public event Action<string>? ErrorMessage;

        public event Action<string>? InformationMessage;

        public event Func<string, string, bool>? ConfirmRequested;

        public event Func<string, string, string>? TextInputRequested;

        public event Func<string, string, string, string>? TextChangeRequested;
    }
}