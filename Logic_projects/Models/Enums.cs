// ===========================================================================
// ФАЙЛ: Enums.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Models
// НАЗНАЧЕНИЕ: Централизованное хранение всех перечислений приложения
// ===========================================================================
// ОСОБЕННОСТИ:
// - Содержит все типы перечислений, используемые в приложении
// - Каждое перечисление имеет атрибуты Description для отображения в UI
// - Включает методы расширения для удобной работы с перечислениями
// ===========================================================================
// СОДЕРЖАНИЕ:
// 1. OrderStatus - статусы заказа
// 2. CustomerType - типы клиентов (физическое/юридическое лицо)
// 3. FilterType - типы фильтров для заказов
// 4. Extension methods - вспомогательные методы для перечислений
// ===========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CustomerOrderApp.WPF.Models
{
    /// <summary>
    /// Статусы заказа
    /// ИСПОЛЬЗУЕТСЯ В: Order.Status
    /// </summary>
    public enum OrderStatus
    {
        [Description("В обработке")]
        [Display(Name = "В обработке", ShortName = "Ожидает")]
        Pending,

        [Description("Оплачен")]
        [Display(Name = "Оплачен", ShortName = "Оплата")]
        Completed,

        [Description("Отменен")]
        [Display(Name = "Отменен", ShortName = "Отмена")]
        Cancelled,

        [Description("Доставлен")]
        [Display(Name = "Доставлен", ShortName = "Доставка")]
        Delivered
    }

    /// <summary>
    /// Типы клиентов
    /// ИСПОЛЬЗУЕТСЯ В: Customer.CustomerType
    /// </summary>
    public enum CustomerType
    {
        [Description("Физическое лицо")]
        Individual,

        [Description("Юридическое лицо")]
        Company
    }

    /// <summary>
    /// Типы фильтров для заказов
    /// ИСПОЛЬЗУЕТСЯ В: OrdersViewModel для фильтрации
    /// </summary>
    public enum FilterType
    {
        [Description("Без фильтра")]
        None,

        [Description("По клиенту")]
        ByCustomer,

        [Description("По дате")]
        ByDate,

        [Description("По статусу")]
        ByStatus,

        [Description("По сумме")]
        ByAmount
    }

    /// <summary>
    /// Режимы редактирования
    /// ИСПОЛЬЗУЕТСЯ В: EditCustomerView/EditOrderView
    /// </summary>
    public enum EditMode
    {
        Create,
        Edit,
        View
    }

    /// <summary>
    /// Методы расширения для работы с перечислениями
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Получение описания перечисления из атрибута Description
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            return value.GetType()
                       .GetMember(value.ToString())
                       .FirstOrDefault()
                       ?.GetCustomAttribute<DescriptionAttribute>()
                       ?.Description
                ?? value.ToString();
        }

        /// <summary>
        /// Получение короткого имени из атрибута Display
        /// </summary>
        public static string GetShortName(this Enum value)
        {
            return value.GetType()
                       .GetMember(value.ToString())
                       .FirstOrDefault()
                       ?.GetCustomAttribute<DisplayAttribute>()
                       ?.ShortName
                ?? value.ToString();
        }

        /// <summary>
        /// Получение полного имени из атрибута Display
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                       .GetMember(value.ToString())
                       .FirstOrDefault()
                       ?.GetCustomAttribute<DisplayAttribute>()
                       ?.Name
                ?? value.ToString();
        }

        /// <summary>
        /// Преобразование строки в перечисление
        /// </summary>
        public static T ToEnum<T>(this string value, T defaultValue = default) where T : Enum
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (item.GetDescription().Equals(value, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return defaultValue;
        }

        /// <summary>
        /// Получение всех значений перечисления с описаниями
        /// </summary>
        public static Dictionary<T, string> GetAllValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(e => e, e => e.GetDescription());
        }
    }
}