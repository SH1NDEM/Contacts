using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ViewModel
{
    public class ContactVM : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        private string _name;
        private string _phoneNumber;
        private string _email;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                ValidateName();
                OnPropertyChanged(nameof(Name));
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                ValidatePhone();
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                ValidateEmail();
                OnPropertyChanged(nameof(Email));
            }
        }

        #region Validation
        public bool HasErrors => _errors.Count > 0;

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName != null && _errors.ContainsKey(propertyName))
                return _errors[propertyName];

            return null;
        }

        private void AddError(string prop, string error)
        {
            if (!_errors.ContainsKey(prop))
                _errors[prop] = new();

            if (!_errors[prop].Contains(error))
            {
                _errors[prop].Add(error);
                ErrorsChanged?.Invoke(this, new(prop));
            }
        }

        private void ClearErrors(string prop)
        {
            if (_errors.Remove(prop))
                ErrorsChanged?.Invoke(this, new(prop));
        }

        private void ValidateName()
        {
            ClearErrors(nameof(Name));

            if (string.IsNullOrWhiteSpace(Name))
                AddError(nameof(Name), "Name is required");

            if (Name?.Length > 100)
                AddError(nameof(Name), "Name must be <= 100 chars");
        }

        private void ValidatePhone()
        {
            ClearErrors(nameof(PhoneNumber));

            if (string.IsNullOrWhiteSpace(PhoneNumber))
                AddError(nameof(PhoneNumber), "Phone number is required");

            if (PhoneNumber?.Length > 100)
                AddError(nameof(PhoneNumber), "Phone number must be <= 100 chars");

            Regex regex = new(@"^[0-9\+\-\(\) ]*$");
            if (!regex.IsMatch(PhoneNumber))
                AddError(nameof(PhoneNumber), "Allowed: digits, + - ( )");
        }

        private void ValidateEmail()
        {
            ClearErrors(nameof(Email));

            if (string.IsNullOrWhiteSpace(Email))
                AddError(nameof(Email), "Email is required");

            if (Email?.Length > 100)
                AddError(nameof(Email), "Email must be <= 100 chars");

            if (!Email.Contains("@"))
                AddError(nameof(Email), "Email must contain '@'");
        }
        #endregion

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new(name));
    }
}
