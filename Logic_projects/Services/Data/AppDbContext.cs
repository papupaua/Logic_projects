// ===========================================================================
// ФАЙЛ: AppDbContext.cs
// РАСПОЛОЖЕНИЕ: CustomerOrderApp.WPF/Services/Data
// НАЗНАЧЕНИЕ: Основной класс для взаимодействия с базой данных через Entity Framework
// ===========================================================================
// ВАЖНЫЕ ЗАМЕЧАНИЯ:
// 1. Этот класс является "мостом" между вашими C#-объектами и базой данных
// 2. Все изменения в структуре классов (моделях) должны отражаться здесь
// 3. Для миграций используйте команды в Package Manager Console:
//    - Enable-Migrations          (только при первом использовании)
//    - Add-Migration [Name]       (после изменений модели)
//    - Update-Database            (применение миграций к БД)
// ===========================================================================

using CustomerOrderApp.WPF.Models;
using Logic_projects.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Runtime.Remoting.Contexts;

namespace CustomerOrderApp.WPF.Services.Data
{
    /// <summary>
    /// Контекст базы данных для приложения учета клиентов и заказов
    /// 
    /// ОСОБЕННОСТИ РЕАЛИЗАЦИИ:
    /// - Наследуется от DbContext (Entity Framework)
    /// - Использует подход Code First
    /// - Автоматические миграции включены для упрощения разработки
    /// 
    /// СТРУКТУРА БАЗЫ ДАННЫХ:
    /// - Таблица Customers (клиенты)
    /// - Таблица Orders (заказы)
    /// - Связь 1:М (один клиент - много заказов)
    /// 
    /// ИНТЕГРАЦИЯ:
    /// - Используется DatabaseService для всех операций с данными
    /// - Строка подключения задается в App.config
    /// </summary>
    public class AppDbContext : DbContext
    {
        // Имя подключения из конфигурационного файла
        private const string CONNECTION_NAME = "DefaultConnection";

        /// <summary>
        /// Конструктор по умолчанию
        /// Инициализирует контекст с подключением из App.config
        /// </summary>
        public AppDbContext() : base(CONNECTION_NAME)
        {
            // Стратегия инициализации для разработки:
            // Автоматически применяет миграции при изменении модели
#if DEBUG
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Migrations.Configuration>());
#else
            Database.SetInitializer<AppDbContext>(null); // Для релиза отключаем
#endif
        }

        // =============================================================
        // КОЛЛЕКЦИИ СУЩНОСТЕЙ (ТАБЛИЦЫ БАЗЫ ДАННЫХ)
        // =============================================================

        /// <summary>
        /// Коллекция клиентов (соответствует таблице Customers)
        /// ДОСТУП:
        ///   var customers = context.Customers.ToList();
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Коллекция заказов (соответствует таблице Orders)
        /// ДОСТУП:
        ///   var orders = context.Orders.Include(o => o.Customer).ToList();
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        // =============================================================
        // КОНФИГУРАЦИЯ МОДЕЛИ БАЗЫ ДАННЫХ
        // =============================================================

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Отключаем автоматическое множественное число имен таблиц
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Настройка таблицы Customers
            modelBuilder.Entity<Customer>()
                .ToTable("Customers") // Имя таблицы в БД
                .HasKey(c => c.Id);   // Первичный ключ

            // Настройка таблицы Orders
            modelBuilder.Entity<Order>()
                .ToTable("Orders")
                .HasKey(o => o.Id);

            // Настройка связи между Клиентом и Заказами
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)     // У клиента много заказов
                .WithRequired(o => o.Customer) // У заказа обязательный клиент
                .HasForeignKey(o => o.CustomerId) // Внешний ключ
                .WillCascadeOnDelete(true);  // Каскадное удаление заказов

            // Настройка индексов для оптимизации запросов
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)      // Индекс для поля Email
                .IsUnique();                 // Уникальность

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Number);    // Индекс для номера заказа

            // Дополнительные настройки полей
            modelBuilder.Entity<Customer>()
                .Property(c => c.FirstName)
                .IsRequired()                // Обязательное поле
                .HasMaxLength(50);            // Ограничение длины

            modelBuilder.Entity<Customer>()
                .Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Order>()
                .Property(o => o.Number)
                .IsRequired()
                .HasMaxLength(20);

            // Важно: Всегда вызываем базовый метод
            base.OnModelCreating(modelBuilder);
        }

        // =============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (ДЛЯ РАЗРАБОТЧИКА)
        // =============================================================

        /// <summary>
        /// Метод для заполнения тестовыми данными (только для разработки)
        /// ИСПОЛЬЗОВАНИЕ:
        ///   using (var context = new AppDbContext())
        ///   {
        ///       context.SeedTestData();
        ///   }
        /// </summary>
        public void SeedTestData()
        {
            // Защита от повторного заполнения
            if (Customers.Any()) return;

            // Создаем тестовых клиентов
            var customer1 = new Customer
            {
                FirstName = "Иван",
                LastName = "Иванов",
                Email = "ivan@example.com",
                Phone = "+79161234567"
            };

            var customer2 = new Customer
            {
                FirstName = "Петр",
                LastName = "Петров",
                Email = "petr@example.com",
                Company = "Рога и копыта"
            };

            Customers.Add(customer1);
            Customers.Add(customer2);

            // Создаем тестовые заказы
            Orders.Add(new Order
            {
                Customer = customer1,
                Number = "ORD-2023-001",
                Total = 1500.50m,
                Status = OrderStatus.Completed
            });

            Orders.Add(new Order
            {
                Customer = customer1,
                Number = "ORD-2023-002",
                Total = 7500.00m,
                Status = OrderStatus.Pending
            });

            Orders.Add(new Order
            {
                Customer = customer2,
                Number = "ORD-2023-003",
                Total = 3000.00m,
                Status = OrderStatus.Cancelled
            });

            // Сохраняем изменения
            SaveChanges();
        }
    }
}