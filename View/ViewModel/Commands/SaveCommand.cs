using Model.Services;
using System;
using System.Windows.Input;
using ViewModel;

namespace ViewModel
{
    public class SaveCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly MainVM _vm;
        private readonly ContactSerializer _serializer;

        public SaveCommand(MainVM vm, ContactSerializer serializer)
        {
            _vm = vm;
            _serializer = serializer;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _serializer.Save(_vm.Contact);
        }
    }
}
