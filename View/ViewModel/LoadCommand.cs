using Model.Services;
using System;
using System.Windows.Input;
using ViewModel;

namespace ViewModel
{
    public class LoadCommand : ICommand
    {
        private readonly MainVM _vm;
        private readonly ContactSerializer _serializer;

        public LoadCommand(MainVM vm, ContactSerializer serializer)
        {
            _vm = vm;
            _serializer = serializer;
        }

        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _vm.Contact = _serializer.Load();
            _vm.RefreshFromContact(); // <-- ЭТО ОБНОВЛЯЕТ UI
        }
    }

}
