// ===========================================================================
// ФАЙЛ: NullToVisibilityConverter.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Converters
// НАЗНАЧЕНИЕ: Конвертер для преобразования null в значение Visibility
// ===========================================================================
// ОСОБЕННОСТИ:
// - Преобразует null в Collapsed, а не-null в Visible (по умолчанию)
// - Поддерживает инвертирование логики через параметр
// - Обрабатывает пустые строки и коллекции как null
// ===========================================================================
// ИСПОЛЬЗОВАНИЕ В XAML:
// <TextBlock Visibility="{Binding Description, 
//                Converter={StaticResource NullToVisibilityConverter}}"/>
//
// ИНВЕРТИРОВАННЫЙ РЕЖИМ:
// <TextBlock Visibility="{Binding HasItems, 
//                Converter={StaticResource NullToVisibilityConverter},
//                ConverterParameter=Invert}"/>
// ===========================================================================

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomerOrderApp.WPF.Converters
{
    /// <summary>
    /// Конвертер, преобразующий значение null в Visibility.Collapsed
    /// 
    /// ПОВЕДЕНИЕ:
    /// - Если значение null -> возвращает Visibility.Collapsed
    /// - Если значение не null -> возвращает Visibility.Visible
    /// 
    /// ДОПОЛНИТЕЛЬНАЯ ЛОГИКА:
    /// - Для строк: пустая строка считается null
    /// - Для коллекций: пустая коллекция считается null
    /// - Для bool: false считается null (только в инвертированном режиме)
    /// 
    /// ИНВЕРТИРОВАНИЕ:
    /// - Если в ConverterParameter передано "Invert", логика меняется:
    ///     * null -> Visible
    ///     * не null -> Collapsed
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Основной метод преобразования
        /// </summary>
        /// <param name="value">Значение для проверки на null</param>
        /// <param name="targetType">Ожидаемый тип (Visibility)</param>
        /// <param name="parameter">
        ///   Опциональные параметры:
        ///     - "Invert": инвертирует логику
        ///     - "Hidden": использует Visibility.Hidden вместо Collapsed
        /// </param>
        /// <param name="culture">Культура (не используется)</param>
        /// <returns>Visibility.Visible или Visibility.Collapsed/Hidden</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, нужно ли инвертировать логику
            bool invert = parameter?.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;

            // Проверяем, нужно ли использовать Hidden вместо Collapsed
            bool useHidden = parameter?.ToString().Equals("Hidden", StringComparison.OrdinalIgnoreCase) == true ||
                            (parameter as string)?.Contains("Hidden") == true;

            // Определяем поведение для разных типов данных
            bool isNull = IsValueNull(value);

            // Основная логика преобразования
            if (invert)
            {
                return isNull ?
                    Visibility.Visible :
                    (useHidden ? Visibility.Hidden : Visibility.Collapsed);
            }

            return isNull ?
                (useHidden ? Visibility.Hidden : Visibility.Collapsed) :
                Visibility.Visible;
        }

        /// <summary>
        /// Определяет, считается ли значение "null" в нашем контексте
        /// </summary>
        private bool IsValueNull(object value)
        {
            if (value == null)
                return true;

            // Для строк: пустая строка = null
            if (value is string str)
                return string.IsNullOrWhiteSpace(str);

            // Для коллекций: пустая коллекция = null
            if (value is ICollection collection)
                return collection.Count == 0;

            // Для IEnumerable (LINQ): пустая последовательность = null
            if (value is IEnumerable enumerable)
                return !enumerable.GetEnumerator().MoveNext();

            // Для bool: false = null (только при инвертировании)
            if (value is bool boolValue)
                return !boolValue;

            // Для nullable-типов
            if (value is Nullable)
                return value.Equals(Activator.CreateInstance(value.GetType()));

            return false;
        }

        /// <summary>
        /// Обратное преобразование (не реализовано)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(
                "Обратное преобразование из Visibility в объект не поддерживается");
        }
    }
}