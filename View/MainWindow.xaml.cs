using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using View.Model;
using ViewModel;

namespace View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();
        }

        private Contact contactBeingEdited;
        private bool isAddingNew = false;


        private void TextBoxClear()
        {
            Name_TextBox.Clear();
            PhoneNumber_TextBox.Clear();
            Email_TextBox.Clear();
        }

        private void TextBoxIsReadOnly(bool boolian)
        {
            Name_TextBox.IsReadOnly = boolian;
            PhoneNumber_TextBox.IsReadOnly = boolian;
            Email_TextBox.IsReadOnly = boolian;
        }

        private void SetMainButtonsEnabled(bool enabled)
        {
            Add_Button.IsEnabled = enabled;
            Edit_Button.IsEnabled = enabled && Name_ListBox.SelectedItem != null;
            Remove_Button.IsEnabled = enabled && Name_ListBox.SelectedItem != null;
        }


        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            Name_ListBox.SelectedItem = null;
            Apply_Button.Visibility = Visibility.Visible;
            TextBoxIsReadOnly(false);
            TextBoxClear();
            isAddingNew = true;
            contactBeingEdited = null;
            SetMainButtonsEnabled(false);
        }

        private void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            Apply_Button.Visibility = Visibility.Hidden;
            TextBoxIsReadOnly(true);

            Contact contactToSelect = null;

            if (contactBeingEdited != null)
            {
                // редактируем существующий контакт
                contactBeingEdited.Name = Name_TextBox.Text;
                contactBeingEdited.PhoneNumber = PhoneNumber_TextBox.Text;
                contactBeingEdited.Email = Email_TextBox.Text;

                Name_ListBox.Items.Refresh();
                contactToSelect = contactBeingEdited;

                contactBeingEdited = null; // выходим из режима редактирования
            }
            else if (isAddingNew)
            {
                // создаём новый контакт
                Contact newContact = new Contact
                {
                    Name = Name_TextBox.Text,
                    PhoneNumber = PhoneNumber_TextBox.Text,
                    Email = Email_TextBox.Text
                };

                Name_ListBox.Items.Add(newContact);
                contactToSelect = newContact;

                isAddingNew = false;
            }

            // Ставим выделение на новый или отредактированный контакт
            if (contactToSelect != null)
            {
                Name_ListBox.SelectedItem = contactToSelect;
            }

            TextBoxIsReadOnly(true);
            Apply_Button.Visibility = Visibility.Hidden;
            SetMainButtonsEnabled(true);
        }


        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            Edit_Button.IsEnabled = true;
            if (Name_ListBox.SelectedItem is Contact contact)
            {
                contactBeingEdited = contact;

                TextBoxIsReadOnly(false);
                Apply_Button.Visibility = Visibility.Visible;
                SetMainButtonsEnabled(false);
            }

        }


        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Name_ListBox.SelectedItem != null)
            {
                Name_ListBox.Items.Remove(Name_ListBox.SelectedItem);
            }
        }

        private void Name_ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Если пользователь создавал новый контакт, но выбрал другой
            if (isAddingNew)
            {
                isAddingNew = false;          // отменяем создание
                TextBoxClear();               // очищаем поля
                Apply_Button.Visibility = Visibility.Hidden;
            }

            // Если редактирование было активно, но выделен новый элемент — отменяем редактирование
            if (contactBeingEdited != null)
            {
                contactBeingEdited = null;
                TextBoxIsReadOnly(true);
                Apply_Button.Visibility = Visibility.Hidden;
            }

            // Получаем выбранный элемент из ListBox
            var selectedItem = Name_ListBox.SelectedItem;
            Edit_Button.IsEnabled = Name_ListBox.SelectedItem != null;
            Remove_Button.IsEnabled = Name_ListBox.SelectedItem != null;

            // Проверяем, что элемент выбран и является объектом типа Contact
            if (selectedItem is Contact contact)
            {
                // Заполняем текстовые поля свойствами выбранного контакта
                Name_TextBox.Text = contact.Name;
                PhoneNumber_TextBox.Text = contact.PhoneNumber;
                Email_TextBox.Text = contact.Email;
            }
            else
            {
                // Если элемент не выбран или это не Contact, очищаем поля
                Name_TextBox.Text = string.Empty;
                PhoneNumber_TextBox.Text = string.Empty;
                Email_TextBox.Text = string.Empty;
            }
        }
    }
}