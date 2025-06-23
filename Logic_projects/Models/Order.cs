// ===========================================================================
// ФАЙЛ: Order.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Models
// НАЗНАЧЕНИЕ: Модель данных, представляющая заказ клиента
// ===========================================================================
// ОСОБЕННОСТИ:
// - Основная сущность для учета заказов
// - Содержит все необходимые свойства согласно требованиям
// - Поддерживает валидацию данных через атрибуты
// - Имеет связь с клиентом через навигационное свойство
// - Включает логику генерации номера заказа
// ===========================================================================
// СВЯЗИ С ДРУГИМИ КОМПОНЕНТАМИ:
// - Entity Framework: сопоставляется с таблицей Orders в БД
// - DatabaseService: используется в CRUD-операциях
// - OrdersViewModel: основная модель для вкладки "Заказы"
// - EditOrderView: форма редактирования заказа
// - StatusToBrushConverter: для визуализации статуса
// ===========================================================================

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOrderApp.WPF.Models
{
    /// <summary>
    /// Класс, представляющий заказ клиента
    /// 
    /// ТРЕБОВАНИЯ К ДАННЫМ:
    /// - Номер заказа обязателен и должен быть уникальным
    /// - Клиент обязателен
    /// - Дата заказа обязательна (по умолчанию текущая дата)
    /// - Сумма должна быть положительной
    /// - Статус обязателен
    /// 
    /// ОГРАНИЧЕНИЯ БАЗЫ ДАННЫХ:
    /// - Номер: 20 символов (уникальный)
    /// - Дата: DATETIME
    /// - Сумма: DECIMAL(18,2)
    /// - Статус: INT (соответствует OrderStatus)
    /// </summary>
    public class Order : ValidatableBase
    {
        private string _number;
        private DateTime _orderDate;
        private decimal _total;
        private OrderStatus _status;

        /// <summary>
        /// Уникальный идентификатор заказа (первичный ключ)
        /// АВТОГЕНЕРАЦИЯ: Значение генерируется базой данных автоматически
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Номер заказа (уникальный идентификатор)
        /// АВТОГЕНЕРАЦИЯ: При создании генерируется автоматически
        /// ФОРМАТ: "ORD-YYYYMMDD-XXXX"
        /// ВАЛИДАЦИЯ:
        ///   - Обязательное поле
        ///   - Максимальная длина 20 символов
        ///   - Должен быть уникальным
        /// </summary>
        [Required(ErrorMessage = "Номер заказа обязателен")]
        [StringLength(20, ErrorMessage = "Номер заказа не может быть длиннее 20 символов")]
        [Display(Name = "Номер заказа", Order = 1)]
        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        /// <summary>
        /// Идентификатор клиента (внешний ключ)
        /// ОБЯЗАТЕЛЬНОЕ: Каждый заказ должен быть связан с клиентом
        /// </summary>
        [Required(ErrorMessage = "Клиент обязателен")]
        [Display(Name = "Клиент", Order = 2)]
        public int CustomerId { get; set; }

        /// <summary>
        /// Клиент, сделавший заказ (навигационное свойство)
        /// ЗАГРУЗКА: По умолчанию не загружается (требуется явное включение)
        /// </summary>
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Дата заказа
        /// ПО УМОЛЧАНИЮ: Текущая дата
        /// ВАЛИДАЦИЯ:
        ///   - Не может быть в будущем
        ///   - Не может быть старше 1 года
        /// </summary>
        [Required(ErrorMessage = "Дата заказа обязательна")]
        [Display(Name = "Дата заказа", Order = 3)]
        [DataType(DataType.Date)]
        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
        }

        /// <summary>
        /// Сумма заказа
        /// ВАЛИДАЦИЯ:
        ///   - Должна быть положительной
        ///   - Максимум 1 000 000
        /// </summary>
        [Required(ErrorMessage = "Сумма заказа обязательна")]
        [Range(0.01, 1_000_000, ErrorMessage = "Сумма должна быть от 0.01 до 1 000 000")]
        [Display(Name = "Сумма", Order = 4)]
        [DataType(DataType.Currency)]
        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        /// <summary>
        /// Статус заказа
        /// ПО УМОЛЧАНИЮ: В обработке (Pending)
        /// ВАЛИДАЦИЯ: Обязательное поле
        /// </summary>
        [Required(ErrorMessage = "Статус заказа обязателен")]
        [Display(Name = "Статус", Order = 5)]
        public OrderStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Комментарий к заказу (дополнительная информация)
        /// </summary>
        [StringLength(500, ErrorMessage = "Комментарий не может быть длиннее 500 символов")]
        [Display(Name = "Комментарий", Order = 6)]
        public string Comment { get; set; }

        /// <summary>
        /// Конструктор по умолчанию
        /// ИНИЦИАЛИЗАЦИЯ:
        ///   - Дата заказа: текущая дата
        ///   - Статус: В обработке
        ///   - Номер: генерируется автоматически
        /// </summary>
        public Order()
        {
            OrderDate = DateTime.Today;
            Status = OrderStatus.Pending;
            Number = GenerateOrderNumber();
        }

        /// <summary>
        /// Генерация номера заказа
        /// ФОРМАТ: "ORD-YYYYMMDD-XXXX" где XXXX - случайное число
        /// ГАРАНТИИ: Гарантирует уникальность в пределах дня
        /// ПРИМЕЧАНИЕ: В реальной системе должен быть более надежный метод
        /// </summary>
        private string GenerateOrderNumber()
        {
            string datePart = DateTime.Today.ToString("yyyyMMdd");
            string randomPart = new Random().Next(1000, 9999).ToString();
            return $"ORD-{datePart}-{randomPart}";
        }

        /// <summary>
        /// Переопределение метода ToString для удобного отображения
        /// ФОРМАТ: "ORD-20230101-1234 (Иванов И.) - 1500 руб."
        /// </summary>
        public override string ToString()
        {
            string customerName = Customer?.LastName ?? "Без клиента";
            return $"{Number} ({customerName}) - {Total:C}";
        }

        /// <summary>
        /// Проверка возможности редактирования заказа
        /// ЗАПРЕЩЕНО РЕДАКТИРОВАТЬ:
        ///   - Оплаченные заказы
        ///   - Отмененные заказы
        /// </summary>
        public bool CanEdit()
        {
            return Status == OrderStatus.Pending;
        }

        /// <summary>
        /// Проверка возможности отмены заказа
        /// МОЖНО ОТМЕНИТЬ:
        ///   - Заказы в обработке
        ///   - Оплаченные, но не доставленные заказы
        /// </summary>
        public bool CanCancel()
        {
            return Status == OrderStatus.Pending ||
                  (Status == OrderStatus.Completed && !IsDelivered);
        }

        /// <summary>
        /// Проверка возможности оплаты заказа
        /// МОЖНО ОПЛАТИТЬ:
        ///   - Только заказы в обработке
        /// </summary>
        public bool CanComplete()
        {
            return Status == OrderStatus.Pending;
        }

        /// <summary>
        /// Вспомогательное свойство для доставки
        /// ПРИМЕЧАНИЕ: Не сохраняется в БД, вычисляется на основе статуса
        /// </summary>
        [NotMapped]
        public bool IsDelivered => Status == OrderStatus.Delivered;
    }
}