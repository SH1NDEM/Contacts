using System;
using System.Windows.Input;

// Простая реализация команды для MVVM
public class RelayCommand : ICommand
{
    // Метод, который будет выполняться при нажатии кнопки
    private readonly Action<object> _execute;

    // Метод, который проверяет, можно ли сейчас выполнить команду
    // Если null, то команда всегда доступна
    private readonly Func<object, bool> _canExecute;

    // Конструктор, принимаем методы для выполнения и проверки возможности
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        if (execute == null) throw new ArgumentNullException(nameof(execute));
        _execute = execute;       // Метод, который вызовем
        _canExecute = canExecute; // Метод проверки (опционально)
    }

    // Проверка, доступна ли команда
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    // Выполнение команды
    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    // Событие, которое WPF использует, чтобы обновлять доступность кнопки
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
