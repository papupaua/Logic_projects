// ===========================================================================
// ФАЙЛ: IDatabaseService.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Services/Data
// НАЗНАЧЕНИЕ: Интерфейс для работы с базой данных
// ===========================================================================
// ОСОБЕННОСТИ:
// - Определяет контракт для всех операций с данными
// - Позволяет легко подменить реализацию (для тестирования, разных СУБД)
// - Асинхронные методы для неблокирующих операций
// ===========================================================================

using CustomerOrderApp.WPF.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerOrderApp.WPF.Services.Data
{
    /// <summary>
    /// Интерфейс сервиса для работы с базой данных
    /// 
    /// ПРИНЦИПЫ:
    /// 1. Разделение обязанностей: отделяет бизнес-логику от доступа к данным
    /// 2. Абстракция: позволяет использовать разные реализации (SQL, NoSQL, InMemory)
    /// 3. Тестируемость: позволяет создавать моки для юнит-тестов
    /// 
    /// РЕКОМЕНДАЦИИ ПО РАСШИРЕНИЮ:
    /// - Добавляйте новые методы по мере необходимости
    /// - Для сложных запросов создавайте специализированные методы
    /// - Избегайте возврата IQueryable (для сохранения контроля над запросами)
    /// </summary>
    public interface IDatabaseService
    {
        // =============================================================
        // ОПЕРАЦИИ С КЛИЕНТАМИ
        // =============================================================

        /// <summary>
        /// Получение всех клиентов
        /// ВАЖНО: Для больших объемов данных реализовать пагинацию
        /// </summary>
        Task<List<Customer>> GetAllCustomersAsync();

        /// <summary>
        /// Поиск клиента по идентификатору
        /// ВОЗВРАЩАЕТ: null если клиент не найден
        /// </summary>
        Task<Customer> GetCustomerByIdAsync(int id);

        /// <summary>
        /// Добавление нового клиента
        /// ПРОВЕРКА: Автоматическая валидация модели
        /// </summary>
        Task AddCustomerAsync(Customer customer);

        /// <summary>
        /// Обновление данных клиента
        /// ОШИБКА: Если клиент не найден
        /// </summary>
        Task UpdateCustomerAsync(Customer customer);

        /// <summary>
        /// Удаление клиента по идентификатору
        /// КАСКАДНОЕ УДАЛЕНИЕ: Все заказы клиента будут удалены
        /// </summary>
        Task DeleteCustomerAsync(int id);

        /// <summary>
        /// Проверка уникальности email
        /// ИСКЛЮЧЕНИЕ: currentId - идентификатор текущего клиента (для исключения из проверки)
        /// </summary>
        Task<bool> IsEmailUniqueAsync(string email, int? currentId = null);

        // =============================================================
        // ОПЕРАЦИИ С ЗАКАЗАМИ
        // =============================================================

        /// <summary>
        /// Получение всех заказов
        /// ВАЖНО: Загружает связанные данные о клиентах
        /// </summary>
        Task<List<Order>> GetAllOrdersAsync();

        /// <summary>
        /// Получение заказов с фильтрацией
        /// ФИЛЬТРЫ: По клиенту, дате, статусу
        /// </summary>
        Task<List<Order>> GetFilteredOrdersAsync(int? customerId = null,
                                               DateTime? startDate = null,
                                               DateTime? endDate = null,
                                               OrderStatus? status = null);

        /// <summary>
        /// Получение заказа по идентификатору
        /// ВОЗВРАЩАЕТ: null если заказ не найден
        /// </summary>
        Task<Order> GetOrderByIdAsync(int id);

        /// <summary>
        /// Добавление нового заказа
        /// ГЕНЕРАЦИЯ: Номер заказа генерируется автоматически
        /// </summary>
        Task AddOrderAsync(Order order);

        /// <summary>
        /// Обновление данных заказа
        /// ОГРАНИЧЕНИЕ: Нельзя изменять оплаченные/отмененные заказы
        /// </summary>
        Task UpdateOrderAsync(Order order);

        /// <summary>
        /// Удаление заказа по идентификатору
        /// </summary>
        Task DeleteOrderAsync(int id);

        /// <summary>
        /// Изменение статуса заказа
        /// ПРОВЕРКА: Разрешены только допустимые переходы статусов
        /// </summary>
        Task ChangeOrderStatusAsync(int orderId, OrderStatus newStatus);

        /// <summary>
        /// Получение статистики по заказам
        /// </summary>
        Task<OrdersSummary> GetOrdersSummaryAsync();

        // =============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // =============================================================

        /// <summary>
        /// Сохранение изменений
        /// ВАЖНО: Использовать для пакетных операций
        /// </summary>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// Статистика по заказам
    /// ИСПОЛЬЗУЕТСЯ: Для отчетов и дашбордов
    /// </summary>
    public class OrdersSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}

    

