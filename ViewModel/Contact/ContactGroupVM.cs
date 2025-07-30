using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Util;
using System.Collections.ObjectModel;

namespace ViewModel.Contact
{
    public partial class ContactGroupVM : ObservableObject
    {
        private ObservableCollection<ContactGroup> _contactGroups = [];
        public ObservableCollection<ContactGroup> ContactGroups { get { return _contactGroups; } }
        private string _backUpGroupName;

        [ObservableProperty]
        private ContactGroup _selectedGroup;

        [ObservableProperty]
        private int _selectedGroupIndex;

        [ObservableProperty]
        public bool _isEnabled = true;
        public IRelayCommand<ContactGroup> DeleteGroupCommand { get; set; }
        public IRelayCommand AddGroupCommand { get; set; }
        public IRelayCommand<ContactGroup> EditGroupCommand { get; set; }
        public EditeState EditeState { get; set; } = EditeState.None;

        public ContactGroupVM(IRelayCommand addGroupCommand, IRelayCommand<ContactGroup> editGroupCommand)
        {
            AddGroupCommand = addGroupCommand;
            EditGroupCommand = editGroupCommand;
            DeleteGroupCommand = new RelayCommand<ContactGroup>(DeleteGroup);
        }

        private void DeleteGroup(ContactGroup? groupToDelete)
        {
            bool? deleteSuccesfull = DeleteDataRequest?.Invoke(groupToDelete);
            if (!deleteSuccesfull.HasValue || deleteSuccesfull == false)
                return;

            var index = SelectedGroupIndex;
            if (index == 0 && ContactGroups.Count == 1)
            {
                SelectedGroupIndex = -1;
                SelectedGroup = null;
                ContactGroups.Clear();
            }
            else
            {
                SelectedGroupIndex--;
                ContactGroups.RemoveAt(index);
            }
        }

        public Func<ContactGroup, bool> DeleteDataRequest;
    }
}
