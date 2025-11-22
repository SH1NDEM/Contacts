using System.ComponentModel;
using View.Model;
using Model.Services;

namespace ViewModel
{
    public class MainVM : INotifyPropertyChanged
    {
        private string _name;
        private string _phoneNumber;
        private string _email;

        public event PropertyChangedEventHandler PropertyChanged;

        public Contact Contact { get; set; }

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

        public SaveCommand SaveCommand { get; }
        public LoadCommand LoadCommand { get; }

        public MainVM()
        {
            Contact = new Contact();

            var serializer = new ContactSerializer();
            SaveCommand = new SaveCommand(this, serializer);
            LoadCommand = new LoadCommand(this, serializer);
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
    }
}
