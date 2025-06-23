// ===========================================================================
// ФАЙЛ: StatusToBrushConverter.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Converters
// НАЗНАЧЕНИЕ: Конвертер для преобразования статуса заказа в визуальное представление
// ===========================================================================
// ОСОБЕННОСТИ:
// - Преобразует значение OrderStatus в соответствующий Brush (цвет)
// - Поддерживает различные режимы отображения: текст, фон, граница
// - Позволяет настраивать цвета через параметры
// ===========================================================================
// ИСПОЛЬЗОВАНИЕ В XAML:
// <TextBlock Text="{Binding Status}" 
//            Foreground="{Binding Status, 
//                Converter={StaticResource StatusToBrushConverter}}"/>
//
// <Border Background="{Binding Status, 
//                Converter={StaticResource StatusToBrushConverter},
//                ConverterParameter=Background}"/>
// ===========================================================================

using CustomerOrderApp.WPF.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomerOrderApp.WPF.Converters
{
    /// <summary>
    /// Конвертер статуса заказа в кисть (Brush) для визуального оформления
    /// 
    /// СТАНДАРТНЫЕ ЦВЕТА:
    /// - Pending (В обработке): Оранжевый (#FFFFA500)
    /// - Completed (Оплачен): Зеленый (#FF008000)
    /// - Cancelled (Отменен): Красный (#FFFF0000)
    /// 
    /// РЕЖИМЫ ОТОБРАЖЕНИЯ (через ConverterParameter):
    /// - Не указан: Цвет текста (Foreground)
    /// - "Background": Цвет фона
    /// - "Border": Цвет границы
    /// - "Light": Светлая версия цвета
    /// - "Dark": Темная версия цвета
    /// </summary>
    [ValueConversion(typeof(OrderStatus), typeof(Brush))]
    public class StatusToBrushConverter : IValueConverter
    {
        // Цвета по умолчанию для различных статусов
        private static readonly SolidColorBrush PendingBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFA500"));
        private static readonly SolidColorBrush CompletedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF008000"));
        private static readonly SolidColorBrush CancelledBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0000"));

        // Светлые версии цветов
        private static readonly SolidColorBrush PendingLightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFF0D0"));
        private static readonly SolidColorBrush CompletedLightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD0FFD0"));
        private static readonly SolidColorBrush CancelledLightBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0D0"));

        // Темные версии цветов
        private static readonly SolidColorBrush PendingDarkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC8400"));
        private static readonly SolidColorBrush CompletedDarkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF006400"));
        private static readonly SolidColorBrush CancelledDarkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC0000"));

        /// <summary>
        /// Основной метод преобразования
        /// </summary>
        /// <param name="value">Значение OrderStatus</param>
        /// <param name="targetType">Ожидаемый тип (Brush)</param>
        /// <param name="parameter">
        ///   Опциональные параметры:
        ///     - "Background": возвращает цвет фона
        ///     - "Border": возвращает цвет границы
        ///     - "Light": светлая версия цвета
        ///     - "Dark": темная версия цвета
        /// </param>
        /// <param name="culture">Культура (не используется)</param>
        /// <returns>Соответствующий Brush для статуса</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is OrderStatus status))
                return Brushes.Black; // Возвращаем по умолчанию черный цвет

            // Определяем запрошенный режим отображения
            string mode = parameter as string ?? string.Empty;
            bool isBackground = mode.Contains("Background");
            bool isBorder = mode.Contains("Border");
            bool useLight = mode.Contains("Light");
            bool useDark = mode.Contains("Dark");

            // Выбираем кисть в зависимости от статуса и режима
            SolidColorBrush brush = status switch
            {
                OrderStatus.Pending => useLight ? PendingLightBrush :
                                       useDark ? PendingDarkBrush : PendingBrush,

                OrderStatus.Completed => useLight ? CompletedLightBrush :
                                         useDark ? CompletedDarkBrush : CompletedBrush,

                OrderStatus.Cancelled => useLight ? CancelledLightBrush :
                                         useDark ? CancelledDarkBrush : CancelledBrush,

                _ => Brushes.Gray // На случай добавления новых статусов
            };

            // Для фона возвращаем светлую версию, если не указано иное
            if (isBackground && !useLight && !useDark)
            {
                brush = status switch
                {
                    OrderStatus.Pending => PendingLightBrush,
                    OrderStatus.Completed => CompletedLightBrush,
                    OrderStatus.Cancelled => CancelledLightBrush,
                    _ => Brushes.LightGray
                };
            }

            // Для границы возвращаем темную версию
            if (isBorder && !useLight && !useDark)
            {
                brush = status switch
                {
                    OrderStatus.Pending => PendingDarkBrush,
                    OrderStatus.Completed => CompletedDarkBrush,
                    OrderStatus.Cancelled => CancelledDarkBrush,
                    _ => Brushes.DarkGray
                };
            }

            return brush;
        }

        /// <summary>
        /// Обратное преобразование (не реализовано)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(
                "Обратное преобразование из Brush в OrderStatus не поддерживается");
        }
    }
}