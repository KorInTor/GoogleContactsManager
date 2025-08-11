using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.PeopleService.v1.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Data;

namespace ViewModel.Contact
{
    public partial class ContactSelectorVM : ObservableObject
    {
        /// <summary>
        ///  <para>Delegate which used to <see cref="Filter"/> visible contacts.</para>
        ///  <para>By default uses <see cref="FilterByTargetPersonsNamesPhoneNumbersGroups(Person)"./></para>
        /// </summary>
        public Predicate<object> FilterPersonsPredicate { get; set; }

        /// <summary>
        /// <para>Person which properties is used to filter <see cref="ContactViewCollection"/>.</para>
        /// <para>When changing <see cref="Person"/> properties call <see cref="Filter"/>.</para>
        /// <para>Calls <see cref="Filter"/> when setted.</para>
        /// </summary>
        [ObservableProperty]
        private Person _targetPerson = new();

        public ListCollectionView ContactViewCollection { get; private set; }

        [ObservableProperty]
        private Person? _selectedPerson;

        [ObservableProperty]
        private int _selectedPersonIndex;

        [ObservableProperty]
        public bool _isEnabled = true;

        public IRelayCommand EditCommand { get; set; }
        public IRelayCommand AddCommand { get; set; }
        public IRelayCommand DeleteCommand { get; set; }

        public ContactSelectorVM(IRelayCommand editCommand, IRelayCommand addCommand, IRelayCommand deleteCommand, List<Person> source)
        {
            EditCommand = editCommand;
            AddCommand = addCommand;
            DeleteCommand = deleteCommand;
            ContactViewCollection = new ListCollectionView(source);
            FilterPersonsPredicate = FilterByTargetPersonsNamesPhoneNumbers;
            ContactViewCollection.Filter = FilterPersonsPredicate;
        }

        partial void OnTargetPersonChanged(Person value)
        {
            ContactViewCollection.Refresh();
        }

        /// <summary>
        /// <para>Base filter for <see cref="ContactViewCollection"/>.</para>
        /// <para>Filters only by <see cref="Name"/>, <see cref="PhoneNumber"/>.</para>
        /// <para>If <see cref="TargetPerson"/> is null all <see cref="Contacts"/> considered valid.</para></para>
        /// </summary>
        /// <param name="personToCheck"></param>
        /// <returns></returns>
        private bool FilterByTargetPersonsNamesPhoneNumbers(object objectPerson)
        {
            if (TargetPerson is null)
            {
                return true;
            }

            if (objectPerson is not Person personToCheck)
                return false;

            var result = false;

            //Группы контактов
            if (TargetPerson.Memberships is not null)
                foreach (var membership in TargetPerson.Memberships)
                {
                    if (membership.ContactGroupMembership is null)
                        continue;
                    if (membership.ContactGroupMembership.ContactGroupResourceName == "contactGroups/all")
                        return true;
                    foreach (var userMemberShip in personToCheck.Memberships)
                    {
                        if (userMemberShip.ContactGroupMembership is null)
                            continue;
                        result |= Matches(userMemberShip.ContactGroupMembership.ContactGroupResourceName,membership.ContactGroupMembership.ContactGroupResourceName);
                    }
                }

            // Имена
            if (personToCheck.Names?.FirstOrDefault() is Name nameToCheck && TargetPerson.Names is not null)
            {
                foreach (var targetName in TargetPerson.Names)
                {
                    bool match =
                        Matches(nameToCheck.FamilyName, targetName.FamilyName) |
                        Matches(nameToCheck.GivenName, targetName.GivenName) |
                        Matches(nameToCheck.MiddleName, targetName.MiddleName) |
                        Matches(nameToCheck.HonorificPrefix, targetName.HonorificPrefix) |
                        Matches(nameToCheck.HonorificSuffix, targetName.HonorificSuffix);

                    result |= match;

                    if (result)
                        return true;
                }
            }

            // Телефоны
            if (personToCheck.PhoneNumbers is not null && TargetPerson.PhoneNumbers is not null)
            {
                foreach (var targetPhoneNumber in TargetPerson.PhoneNumbers)
                {
                    foreach (var sourcePhoneNumber in personToCheck.PhoneNumbers)
                    {
                        bool match = Matches(sourcePhoneNumber.Value, targetPhoneNumber.Value);

                        result |= match;
                        if (result)
                        {
                            return true;
                        }
                    }
                }
            }

            return result;
        }

        private static bool Matches(string source, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;
            if (string.IsNullOrEmpty(source))
                return false;

            return source.Contains(filter, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
