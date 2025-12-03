using Model.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using View.Model;
using ViewModel.Commands;

namespace ViewModel
{
    public class MainVM : INotifyPropertyChanged
    {
        private string _name;
        private string _phoneNumber;
        private string _email;
        private Contact _selectedContact;

        public event PropertyChangedEventHandler PropertyChanged;

        public Contact Contact { get; set; }
        public bool IsEditing { get; set; }
        public bool IsAddingNew { get; set; }
        public ICommand AddCommand { get; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Contact.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                Contact.PhoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                Contact.Email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public Contact SelectedContact
        {
            get => _selectedContact;
            set
            {
                _selectedContact = value;
                if (!IsEditing && !IsAddingNew)
                {
                    if (value != null)
                    {
                        Name = value.Name;
                        PhoneNumber = value.PhoneNumber;
                        Email = value.Email;
                    }
                }

                OnPropertyChanged(nameof(SelectedContact));
            }
        }

        public SaveCommand SaveCommand { get; }
        public LoadCommand LoadCommand { get; }
        public AddContact AddContact { get; }
        //public AddContact RemoveContact { get; }
        //public AddContact EditContact { get; }
        //public AddContact ListBoxSelectionChanged { get; }


        public MainVM()
        {
            Contact = new Contact();


            var serializer = new ContactSerializer();
            SaveCommand = new SaveCommand(this, serializer);
            LoadCommand = new LoadCommand(this, serializer);
            AddContact = new AddContact(ExecuteAddContact);
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        // Метод для обновления UI после Load()
        public void RefreshFromContact()
        {
            Name = Contact.Name;
            PhoneNumber = Contact.PhoneNumber;
            Email = Contact.Email;
        }

        private void ExecuteAddContact(object obj)
        {
            SelectedContact = null;

            Name = "";
            PhoneNumber = "";
            Email = "";

            IsEditing = true;
            IsAddingNew = true;

            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsAddingNew));
        }
    }
}
