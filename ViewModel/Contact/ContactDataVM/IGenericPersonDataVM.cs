namespace ViewModel.Contact.ContactDataVM
{
    public interface IGenericPersonDataVM
    {
        public string FieldMask { get; }
        public bool IsDataChanged { get; }
        public bool IsDataReadOnly { get; set; }
    }
}
