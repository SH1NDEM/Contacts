using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using View.Model;
using View.Services;
using ViewModel.Commands;
using System.Collections;
using System.Collections.Generic;


namespace ViewModel
{
    public class MainVM : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        private string _name;
        private string _phoneNumber;
        private string _email;
        private Contact _selectedContact;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ContactSerializer _serializer;
        private const string DataFile = "contacts.json";

        private Contact _tempContact;
        public Contact TempContact
        {
            get => _tempContact;
            set
            {
                _tempContact = value;
                OnPropertyChanged(nameof(TempContact));
            }
        }

        public ObservableCollection<Contact> Contacts { get; set; }
            = new ObservableCollection<Contact>();

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
        public RelayCommand EditContact { get; }

        // ----------------------------
        // ВАЛИДАЦИЯ СВОЙСТВ
        // ----------------------------
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                TempContact.Name = value;
                ValidateName();
                OnPropertyChanged(nameof(Name));
                ApplyContact.RaiseCanExecuteChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                TempContact.PhoneNumber = value;
                ValidatePhone();
                OnPropertyChanged(nameof(PhoneNumber));
                ApplyContact.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                TempContact.Email = value;
                ValidateEmail();
                OnPropertyChanged(nameof(Email));
                ApplyContact.RaiseCanExecuteChanged();
            }
        }

        // ----------------------------
        // SelectedContact
        // ----------------------------
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
                ApplyContact.RaiseCanExecuteChanged();
            }
        }

        // ----------------------------
        // CONSTRUCTOR
        // ----------------------------
        public MainVM()
        {
            _serializer = new ContactSerializer(DataFile);

            var loaded = _serializer.Load();
            Contacts = new ObservableCollection<Contact>(loaded);
            TempContact = new Contact();
            Contact = new Contact();

            AddContact = new RelayCommand(ExecuteAddContact);
            ApplyContact = new RelayCommand(ExecuteApplyContact, CanApplyContact);
            RemoveContact = new RelayCommand(ExecuteRemoveContact, CanRemoveContact);
            EditContact = new RelayCommand(ExecuteEditContact, CanEditContact);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Проверяем доступность Apply при изменении полей
            if (propertyName == nameof(Name) ||
                propertyName == nameof(PhoneNumber) ||
                propertyName == nameof(Email))
            {
                ApplyContact?.RaiseCanExecuteChanged();
            }
        }

        // ==============================
        // ВАЛИДАЦИЯ
        // ==============================

        private void AddError(string prop, string msg)
        {
            if (!_errors.ContainsKey(prop))
                _errors[prop] = new List<string>();

            if (!_errors[prop].Contains(msg))
                _errors[prop].Add(msg);

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }

        private void ClearErrors(string prop)
        {
            if (_errors.Remove(prop))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }

        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName != null && _errors.ContainsKey(propertyName))
                return _errors[propertyName];

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Валидация имени.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private void ValidateName()
        {
            ClearErrors(nameof(Name));
            if (!string.IsNullOrWhiteSpace(Name))
            {
                if (Name.Length > 100)
                    AddError(nameof(Name), "Name must be ≤ 100 characters");
            }
        }

        /// <summary>
        /// Валидация номера телефона.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private void ValidatePhone()
        {
            ClearErrors(nameof(PhoneNumber));
            if (!string.IsNullOrWhiteSpace(PhoneNumber))
            {
                if (PhoneNumber.Length > 100)
                    AddError(nameof(PhoneNumber), "Phone number must be ≤ 100 characters");

                if (!PhoneNumber.All(c => char.IsDigit(c) || "+-() ".Contains(c)))
                    AddError(nameof(PhoneNumber), "Invalid characters in phone number");
            }
        }

        /// <summary>
        /// Валидация почты.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private void ValidateEmail()
        {
            ClearErrors(nameof(Email));
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (Email.Length > 100)
                    AddError(nameof(Email), "Email must be ≤ 100 characters");

                if (!Email.Contains("@"))
                    AddError(nameof(Email), "Email must contain '@'");
            }
        }

        /// <summary>
        /// Значение на возможность добавления контакта контанта.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanApplyContact(object obj)
        {
            bool anyFieldFilled = !string.IsNullOrWhiteSpace(Name) ||
                                  !string.IsNullOrWhiteSpace(PhoneNumber) ||
                                  !string.IsNullOrWhiteSpace(Email);

            return IsEditing && anyFieldFilled && !HasErrors;
        }

        // ==============================
        // COMMAND LOGIC
        // ==============================

        /// <summary>
        /// Открытие полей на запись и видимости кнопки Apply.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private void ExecuteAddContact(object obj)
        {
            SelectedContact = new Contact();

            Name = "";
            PhoneNumber = "";
            Email = "";

            IsEditing = true;
            IsAddingNew = true;

            ApplyContact.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Значение на возможность удаления контанта.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanRemoveContact(object obj) => SelectedContact != null;

        /// <summary>
        /// Выполнение удаления контакта.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private void ExecuteRemoveContact(object obj)
        {
            if (SelectedContact != null)
            {
                Contacts.Remove(SelectedContact);
                SelectedContact = null;

                IsEditing = false;
                IsAddingNew = false;

                _serializer.Save(Contacts);
                ApplyContact.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Значение на возможность изменения контанта.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanEditContact(object obj) => SelectedContact != null;

        /// <summary>
        /// Выполнение команды изменения контакта.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteEditContact(object obj)
        {
            if (SelectedContact == null)
                return;

            TempContact = new Contact
            {
                Name = SelectedContact.Name,
                PhoneNumber = SelectedContact.PhoneNumber,
                Email = SelectedContact.Email
            };

            IsEditing = true;
            IsAddingNew = false;
            ApplyContact.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Выполение команды сохраниения контакта.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteApplyContact(object obj)
        {
            if (IsAddingNew && Contact != null)
            {
                Contacts.Add(TempContact);
            }
            else
            {
                if (SelectedContact != null)
                {
                    SelectedContact.Name = Name;
                    SelectedContact.PhoneNumber = PhoneNumber;
                    SelectedContact.Email = Email;
                    _serializer.Save(Contacts);
                }
                var index = Contacts.IndexOf(SelectedContact);
                Contacts[index] = TempContact;
            }

            IsAddingNew = false;
            IsEditing = false;
            _serializer.Save(Contacts);
            ApplyContact.RaiseCanExecuteChanged();
            ResetTempContact();
        }

        private void ResetTempContact()
        {
            TempContact = new Contact();
            Name = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
        }

    }
}
