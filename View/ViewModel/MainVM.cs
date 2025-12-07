
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using View.Model;
using View.Services;
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

        private readonly ContactSerializer _serializer;
        private const string DataFile = "contacts.json";


        // Коллекция контактов — для привязки к ListBox/ListView
        public ObservableCollection<Contact> Contacts { get; set; }
            = new ObservableCollection<Contact>();

        // Текущий контакт, который редактируется
        public Contact Contact { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }

        public bool IsAddingNew { get; set; }
        public RelayCommand AddContact { get; }
        public RelayCommand ApplyContact { get; }
        public RelayCommand RemoveContact { get; }

        /// <summary>
        /// Команда редактирования контакта.
        /// </summary>
        public RelayCommand EditContact { get; }

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
                if (_selectedContact == value)
                    return;

                if (IsEditing)
                {
                    IsEditing = false;
                    IsAddingNew = false;

                    if (_selectedContact != null)
                    {
                        Name = _selectedContact.Name;
                        PhoneNumber = _selectedContact.PhoneNumber;
                        Email = _selectedContact.Email;
                    }
                }

                _selectedContact = value;

                if (_selectedContact != null)
                {
                    Contact = _selectedContact;
                    Name = _selectedContact.Name;
                    PhoneNumber = _selectedContact.PhoneNumber;
                    Email = _selectedContact.Email;
                }

                OnPropertyChanged(nameof(SelectedContact));
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAddingNew));
            }
        }


        public MainVM()
        {
            _serializer = new ContactSerializer(DataFile);

            var loaded = _serializer.Load();
            Contacts = new ObservableCollection<Contact>(loaded);

            Contact = new Contact();

            // команды
            AddContact = new RelayCommand(ExecuteAddContact);
            ApplyContact = new RelayCommand(ExecuteApplyContact);
            RemoveContact = new RelayCommand(ExecuteRemoveContact, CanRemoveContact);
            EditContact = new RelayCommand(ExecuteEditContact, CanEditContact);
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Обновление UI после Load()
        public void RefreshFromContact()
        {
            Name = Contact.Name;
            PhoneNumber = Contact.PhoneNumber;
            Email = Contact.Email;
        }

        private void ExecuteAddContact(object obj)
        {
            SelectedContact = new Contact();

            Name = "";
            PhoneNumber = "";
            Email = "";

            IsEditing = true;
            IsAddingNew = true;

            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsAddingNew));
        }

        // Метод сохранения контакта в коллекцию
        public void AddCurrentContactToList(object obj)
        {
            if (Contact != null)
                Contacts.Add(Contact);

            IsEditing = false;
            IsAddingNew = false;
        }

        private bool CanRemoveContact(object obj)
        {
            return SelectedContact != null;
        }

        private void ExecuteRemoveContact(object obj)
        {
            if (SelectedContact != null)
            {
                Contacts.Remove(SelectedContact);

                // После удаления убираем выделение
                SelectedContact = null;

                // Блокируем режим редактирования
                IsEditing = false;
                IsAddingNew = false;

                _serializer.Save(Contacts);

                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAddingNew));
            }
        }

        private bool CanEditContact(object obj)
        {
            return SelectedContact != null;
        }


        private void ExecuteEditContact(object obj)
        {
            if (SelectedContact == null)
                return;

            // Устанавливаем текущий редактируемый объект
            Contact = SelectedContact;

            // Загружаем данные в поля
            Name = Contact.Name;
            PhoneNumber = Contact.PhoneNumber;
            Email = Contact.Email;

            // Входим в режим редактирования
            IsEditing = true;
            IsAddingNew = false;

            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsAddingNew));
        }

        private void ExecuteApplyContact(object obj)
        {
            if (IsAddingNew)
            {
                if (Contact != null)
                    Contacts.Add(Contact);

                IsAddingNew = false;
            }

            IsEditing = false;

            _serializer.Save(Contacts);

            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsAddingNew));
        }
    }
}
