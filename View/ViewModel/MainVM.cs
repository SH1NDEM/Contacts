using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using View.Model;
using View.Services;

namespace ViewModel
{
    public partial class MainVM : ObservableObject, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public ObservableCollection<ContactVM> Contacts { get; } = new();

        [ObservableProperty]
        private ContactVM tempContact = new ContactVM();

        private readonly ContactSerializer _serializer = new("contacts.json");

        [ObservableProperty]
        private string name;

        partial void OnNameChanged(string value)
        {
            TempContact.Name = value;
            ValidateName();
            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string phoneNumber;

        partial void OnPhoneNumberChanged(string value)
        {
            TempContact.PhoneNumber = value;
            ValidatePhone();
            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string email;

        partial void OnEmailChanged(string value)
        {
            TempContact.Email = value;
            ValidateEmail();
            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private ContactVM selectedContact;

        partial void OnSelectedContactChanged(ContactVM value)
        {
            if (IsEditing)
            {
                IsEditing = false;
                IsAddingNew = false;
            }

            if (value != null)
            {
                TempContact = new ContactVM
                {
                    Name = value.Name,
                    PhoneNumber = value.PhoneNumber,
                    Email = value.Email
                };

                Name = value.Name;
                PhoneNumber = value.PhoneNumber;
                Email = value.Email;
            }

            RemoveContactCommand.NotifyCanExecuteChanged();
            EditContactCommand.NotifyCanExecuteChanged();
            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private bool isAddingNew;

        #region Commands
        [RelayCommand]
        private void AddContact()
        {
            SelectedContact = null;
            TempContact = new ContactVM();

            Name = "";
            PhoneNumber = "";
            Email = "";

            IsAddingNew = true;
            IsEditing = true;

            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanRemoveContact))]
        private void RemoveContact()
        {
            if (SelectedContact == null) return;

            int index = Contacts.IndexOf(SelectedContact);
            Contacts.Remove(SelectedContact);

            if (Contacts.Count == 0)
                SelectedContact = null;
            else if (index >= Contacts.Count)
                SelectedContact = Contacts.Last();
            else
                SelectedContact = Contacts[index];

            _serializer.Save(Contacts.Select(c => new Contact
            {
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            }).ToList());
        }

        private bool CanRemoveContact() => SelectedContact != null;

        [RelayCommand(CanExecute = nameof(CanEditContact))]
        private void EditContact()
        {
            if (SelectedContact == null) return;

            TempContact = new ContactVM
            {
                Name = SelectedContact.Name,
                PhoneNumber = SelectedContact.PhoneNumber,
                Email = SelectedContact.Email
            };

            Name = TempContact.Name;
            PhoneNumber = TempContact.PhoneNumber;
            Email = TempContact.Email;

            IsEditing = true;
            IsAddingNew = false;

            ApplyContactCommand.NotifyCanExecuteChanged();
        }

        private bool CanEditContact() => SelectedContact != null;

        [RelayCommand(CanExecute = nameof(CanApplyContact))]
        private void ApplyContact()
        {
            if (IsAddingNew)
            {
                Contacts.Add(new ContactVM
                {
                    Name = Name,
                    PhoneNumber = PhoneNumber,
                    Email = Email
                });
            }
            else if (SelectedContact != null)
            {
                SelectedContact.Name = Name;
                SelectedContact.PhoneNumber = PhoneNumber;
                SelectedContact.Email = Email;
            }

            _serializer.Save(Contacts.Select(c => new Contact
            {
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            }).ToList());

            IsEditing = false;
            IsAddingNew = false;
        }

        private bool CanApplyContact() =>
            IsEditing &&
            !HasErrors &&
            (!string.IsNullOrWhiteSpace(Name) ||
             !string.IsNullOrWhiteSpace(PhoneNumber) ||
             !string.IsNullOrWhiteSpace(Email));
        #endregion

        #region Validation
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (propertyName != null && _errors.ContainsKey(propertyName))
                return _errors[propertyName];

            return Enumerable.Empty<string>();
        }

        private void AddError(string prop, string msg)
        {
            if (!_errors.ContainsKey(prop))
                _errors[prop] = new List<string>();

            if (!_errors[prop].Contains(msg))
            {
                _errors[prop].Add(msg);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            }
        }

        private void ClearErrors(string prop)
        {
            if (_errors.Remove(prop))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }

        private void ValidateName()
        {
            ClearErrors(nameof(Name));
            if (Name?.Length > 100)
                AddError(nameof(Name), "Имя ≤ 100 символов.");
        }

        private void ValidatePhone()
        {
            ClearErrors(nameof(PhoneNumber));
            if (PhoneNumber?.Length > 100)
                AddError(nameof(PhoneNumber), "Телефон ≤ 100 символов.");
            if (PhoneNumber != null && !PhoneNumber.All(c => char.IsDigit(c) || "+-() ".Contains(c)))
                AddError(nameof(PhoneNumber), "Недопустимые символы.");
        }

        private void ValidateEmail()
        {
            ClearErrors(nameof(Email));
            if (Email?.Length > 100)
                AddError(nameof(Email), "Email ≤ 100 символов.");
            if (!string.IsNullOrEmpty(Email) && !Email.Contains("@"))
                AddError(nameof(Email), "Неверный формат email.");
        }
        #endregion
    }
}
