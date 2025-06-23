// ===========================================================================
// ФАЙЛ: Customer.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Models
// НАЗНАЧЕНИЕ: Модель данных, представляющая клиента компании
// ===========================================================================
// ОСОБЕННОСТИ:
// - Основная сущность для учета клиентов
// - Содержит все необходимые свойства согласно требованиям
// - Поддерживает валидацию данных через атрибуты
// - Имеет навигационное свойство для связанных заказов
// ===========================================================================
// СВЯЗИ С ДРУГИМИ КОМПОНЕНТАМИ:
// - Entity Framework: сопоставляется с таблицей Customers в БД
// - DatabaseService: используется в CRUD-операциях
// - CustomersViewModel: основная модель для вкладки "Клиенты"
// - EditCustomerView: форма редактирования свойств клиента
// ===========================================================================

using Logic_projects.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows.Controls;

namespace CustomerOrderApp.WPF.Models
{
    /// <summary>
    /// Класс, представляющий клиента компании
    /// 
    /// ТРЕБОВАНИЯ К ДАННЫМ:
    /// - Имя и фамилия обязательны
    /// - Email должен быть уникальным и соответствовать формату
    /// - Телефон должен соответствовать формату
    /// - Компания может быть пустой
    /// 
    /// ОГРАНИЧЕНИЯ БАЗЫ ДАННЫХ:
    /// - Имя: 50 символов
    /// - Фамилия: 50 символов
    /// - Email: 100 символов (уникальный)
    /// - Телефон: 20 символов
    /// - Компания: 100 символов
    /// </summary>
    public class Customer : ValidatableBase
    {
        /// <summary>
        /// Уникальный идентификатор клиента (первичный ключ)
        /// АВТОГЕНЕРАЦИЯ: Значение генерируется базой данных автоматически
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Имя клиента (обязательное поле)
        /// ВАЛИДАЦИЯ:
        ///   - Не может быть пустым
        ///   - Максимальная длина 50 символов
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
        [Display(Name = "Имя", Order = 1)]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия клиента (обязательное поле)
        /// ВАЛИДАЦИЯ:
        ///   - Не может быть пустой
        ///   - Максимальная длина 50 символов
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
        [Display(Name = "Фамилия", Order = 2)]
        public string LastName { get; set; }

        /// <summary>
        /// Полное имя клиента (вычисляемое свойство)
        /// ФОРМАТ: "Фамилия Имя"
        /// НЕ СОХРАНЯЕТСЯ В БАЗЕ ДАННЫХ
        /// </summary>
        [NotMapped]
        [Display(Name = "ФИО", Order = 3)]
        public string FullName => $"{LastName} {FirstName}";

        /// <summary>
        /// Электронная почта клиента
        /// ВАЛИДАЦИЯ:
        ///   - Должна соответствовать формату email
        ///   - Максимальная длина 100 символов
        ///   - Должна быть уникальной (проверка на уровне БД)
        /// </summary>
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(100, ErrorMessage = "Email не может быть длиннее 100 символов")]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        /// <summary>
        /// Телефонный номер клиента
        /// ВАЛИДАЦИЯ:
        ///   - Должен соответствовать формату телефона
        ///   - Максимальная длина 20 символов
        /// </summary>
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [StringLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        [Display(Name = "Телефон", Order = 5)]
        public string Phone { get; set; }

        /// <summary>
        /// Название компании (если клиент является представителем компании)
        /// ВАЛИДАЦИЯ:
        ///   - Максимальная длина 100 символов
        /// </summary>
        [StringLength(100, ErrorMessage = "Название компании не может быть длиннее 100 символов")]
        [Display(Name = "Компания", Order = 6)]
        public string Company { get; set; }

        /// <summary>
        /// Коллекция заказов, связанных с этим клиентом
        /// СВЯЗЬ: Один ко многим (один клиент - много заказов)
        /// ВАЖНО: При удалении клиента все его заказы удаляются каскадно
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; } = new ObservableCollection<Order>();

        /// <summary>
        /// Переопределение метода ToString для удобного отображения
        /// ФОРМАТ: "Иванов Иван (ivan@example.com)"
        /// </summary>
        public override string ToString()
        {
            return $"{LastName} {FirstName} {(string.IsNullOrWhiteSpace(Email) ? "" : $"({Email})"}";
        }
    }

    /// <summary>
    /// Базовый класс для объектов, поддерживающих валидацию
    /// РЕАЛИЗУЕТ: Интерфейс INotifyDataErrorInfo для валидации в WPF
    /// </summary>
    public abstract class ValidatableBase : INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => _errors.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errors.Values.SelectMany(errors => errors);
            }

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : Enumerable.Empty<string>();
        }

        protected void ValidateProperty(string propertyName, object value)
        {
            var validationContext = new ValidationContext(this) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateProperty(value, validationContext, validationResults);

            // Удаление старых ошибок
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
            }

            // Добавление новых ошибок
            if (validationResults.Any())
            {
                _errors[propertyName] = validationResults.Select(r => r.ErrorMessage).ToList();
            }

            // Уведомление об изменениях
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}