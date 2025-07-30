using CommunityToolkit.Mvvm.ComponentModel;
using Google.Apis.PeopleService.v1.Data;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using ViewModel.Contact.ContactDataVM;

namespace ViewModel.Contact
{
    public partial class ContactGroupPopupVM : ObservableObject, IGenericPersonDataVM
    {
        private string _jsonBackup;

        public IList<Membership> ActualValues
        {
            get
            {
                List<Membership> newMemberShip = [];
                SelectableGroups.Where(sGroup => sGroup.IsSelected == true)
                    .Select(sGroup => sGroup.Value)
                    .ToList()
                    .ForEach(sGroup => newMemberShip
                    .Add(new Membership { ContactGroupMembership = new ContactGroupMembership() { ContactGroupResourceName = sGroup.ResourceName } })
                    );
                return newMemberShip;
            }
            set => RebuildSelectableGroupsAndCache(value);
        }

        [ObservableProperty]
        public IList<ContactGroup> _possibleValues;

        public ObservableCollection<SelectableObject<ContactGroup>> SelectableGroups { get; private set; } = [];

        public string FieldMask => "memberships";

        public bool IsDataChanged => JsonConvert.SerializeObject(ActualValues) != _jsonBackup;

        public bool IsDataReadOnly { get; set; }

        public ContactGroupPopupVM(IList<ContactGroup> possibleValues, IList<Membership> actualValues)
        {
            PossibleValues = possibleValues;
            RebuildSelectableGroupsAndCache(actualValues);
        }

        public ContactGroupPopupVM()
        {
        }

        partial void OnPossibleValuesChanged(IList<ContactGroup> value)
        {
            RebuildSelectableGroupsAndCache(ActualValues);
        }

        private void RebuildSelectableGroupsAndCache(IList<Membership> selectedGroups)
        {
            if (selectedGroups is null)
            {
                selectedGroups = [];
                selectedGroups.Add(new Membership() { ContactGroupMembership = new() { ContactGroupResourceName= "contactGroups/myContacts" } });
            }
            SelectableGroups.Clear();
            _jsonBackup = JsonConvert.SerializeObject(selectedGroups);
            foreach (var possible in PossibleValues)
            {
                if (possible.ResourceName == "contactGroups/all")
                    continue;
                bool isSelected = selectedGroups.Any(sel => sel.ContactGroupMembership.ContactGroupResourceName == possible.ResourceName);
                SelectableGroups.Add(new(possible, isSelected));
            }
        }
    }
}
