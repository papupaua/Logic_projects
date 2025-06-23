using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace LogicCalculator
{
    public class CalculatorViewModel : INotifyPropertyChanged
    {
        private string _displayText = "0";
        private bool _isNewCalculation = true;

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        public ICommand DigitCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand DeleteCommand { get; }

        public CalculatorViewModel()
        {
            DigitCommand = new RelayCommand<string>(AppendDigit);
            OperationCommand = new RelayCommand<string>(AppendOperation);
            EqualsCommand = new RelayCommand(CalculateResult);
            ClearCommand = new RelayCommand(ClearDisplay);
            DeleteCommand = new RelayCommand(DeleteLastChar);
        }

        private void AppendDigit(string digit)
        {
            if (_isNewCalculation)
            {
                DisplayText = digit;
                _isNewCalculation = false;
            }
            else
            {
                DisplayText += digit;
            }
        }

        private void AppendOperation(string operation)
        {
            if (_isNewCalculation && operation != "(")
            {
                DisplayText = "0" + operation;
            }
            else
            {
                DisplayText += operation;
            }
            _isNewCalculation = false;
        }

        private void ClearDisplay()
        {
            DisplayText = "0";
            _isNewCalculation = true;
        }

        private void DeleteLastChar()
        {
            if (DisplayText.Length > 1)
            {
                DisplayText = DisplayText[..^1];
            }
            else
            {
                DisplayText = "0";
                _isNewCalculation = true;
            }
        }

        private void CalculateResult()
        {
            try
            {
                var sanitized = SanitizeExpression(DisplayText);
                var result = EvaluateExpression(sanitized);
                DisplayText = result.ToString();
                _isNewCalculation = true;
            }
            catch (Exception ex)
            {
                DisplayText = "Error";
                _isNewCalculation = true;
            }
        }

        private string SanitizeExpression(string input)
        {
            // Заменяем логические операторы на вычисляемые аналоги
            return input.Replace("&&", "&")
                       .Replace("||", "|")
                       .Replace("!", "NOT ");
        }

        private double EvaluateExpression(string expression)
        {
            // Удаляем опасные символы, оставляя только нужные для вычислений
            var safeExpr = Regex.Replace(expression, @"[^\d\.\+\-\*\/&|\(\) NOT]", "");
            
            // Обработка NOT оператора
            safeExpr = safeExpr.Replace("NOT", "!");

            using (DataTable dt = new DataTable())
            {
                var result = dt.Compute(safeExpr, null);
                return Convert.ToDouble(result);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => 
            _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
