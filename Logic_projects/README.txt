ViewModels/
├── MainViewModel.cs          # Координатор главного окна
├── CustomersViewModel.cs     # Управление клиентами (готово)
├── OrdersViewModel.cs        # Управление заказами (частично)
├── EditCustomerViewModel.cs  # Редактирование клиента (готово)
├── EditOrderViewModel.cs     # Редактирование заказа (готово)
└── ViewModelBase.cs          # Базовый класс VM

Services/
├── Data/
│   ├── IDatabaseService.cs   # Интерфейс работы с БД
│   ├── DatabaseService.cs    # Реализация EF для SQL Server
│   └── AppDbContext.cs       # Контекст БД
└── UI/
    ├── IDialogService.cs     # Интерфейс диалогов
    └── DialogService.cs      # Реализация диалогов (частично)

Models/
├── Customer.cs               # Сущность клиента
├── Order.cs                  # Сущность заказа
└── Enums.cs                  # Перечисления



Компонент	Статус	Заметки
CustomersViewModel	✅ Готово	Полный CRUD
OrdersViewModel	⚠️ Частично	Не реализовано изменение статуса
EditCustomerViewModel	✅ Готово	С валидацией
EditOrderViewModel	✅ Готово	С валидацией
DatabaseService	✅ Готово	Полное покрытие IDatabaseService
DialogService	⚠️ Частично	Заглушки для заказов 
там не все готово еще много доделывать, но больше разбираться скорее всего будет много ошибок 

🔧 Заглушки (TODOs)
OrdersViewModel.ChangeOrderStatus()

csharp
// ВРЕМЕННАЯ ЗАГЛУШКА
private void ChangeOrderStatus(object _)
{
    _dialogService.ShowError("Функция изменения статуса в разработке", "Инфо");
    /* ПЛАНИРУЕМАЯ РЕАЛИЗАЦИЯ:
    var newStatus = DetermineNextStatus(selectedOrder.Status);
    await _dbService.ChangeOrderStatusAsync(...);
    */
}
DialogService.ShowEditOrderDialog()

csharp
// ВРЕМЕННАЯ ЗАГЛУШКА
public bool ShowEditOrderDialog(Order order)
{
    MessageBox.Show("Заглушка для диалога заказов");
    return true; // Для тестирования потока
}
DatabaseService.SaveChangesAsync()

csharp
// НЕ РЕАЛИЗОВАНО ДЛЯ ПАКЕТНЫХ ОПЕРАЦИЙ
public Task SaveChangesAsync()
{
    throw new NotImplementedException("Пакетное сохранение не поддерживается");
}




📦 Зависимости (NuGet)
Библиотека	Версия	Назначение
EntityFramework	6.4.4	ORM для SQL Server
Microsoft.Extensions.DependencyInjection	7.0.0	DI-контейнер
CommunityToolkit.Mvvm	8.2.0	RelayCommand и MVVM-утилиты
System.ComponentModel.Annotations	5.0.0	Валидация данных



🧩 Интеграция с другими блоками
Главное окно (MainWindow.xaml):

xml
<TabControl>
    <TabItem Header="Клиенты">
        <views:CustomersView DataContext="{Binding CustomersVM}"/>
    </TabItem>
    <TabItem Header="Заказы">
        <views:OrdersView DataContext="{Binding OrdersVM}"/>
    </TabItem>
</TabControl>
Представление заказов (OrdersView.xaml):

xaml
<Button Content="Изменить статус" 
        Command="{Binding ChangeStatusCommand}"
        IsEnabled="{Binding OrdersView.CurrentItem}"/>
DI-контейнер (App.xaml.cs):

csharp
protected override void OnStartup(StartupEventArgs e)
{
    var services = new ServiceCollection();
    
    // Регистрация ВСЕХ компонентов
    services.AddSingleton<IDatabaseService, DatabaseService>();
    services.AddSingleton<IDialogService, DialogService>();
    services.AddTransient<MainViewModel>();
    // ... другие ViewModels
    
    var provider = services.BuildServiceProvider();
    MainWindow = new MainWindow {
        DataContext = provider.GetRequiredService<MainViewModel>()
    };
    MainWindow.Show();
}


⚠️ Известные проблемы
OrdersViewModel:

Фильтрация не обновляется при изменении данных

Нет обработки длительных операций

csharp
// Временное решение:
IsLoading = true;
try { /* операции */ }
finally { IsLoading = false; }
Диалоги:

Нет реализации окна редактирования заказов

Подтверждение удаления без кастомного диалога

🔮 Следующие шаги для команды
Разработчикам UI:

Реализовать EditOrderView.xaml

Создать диалог изменения статуса

Добавить прогресс-бар для операций загрузки

Разработчику сервисов:

Реализовать ShowEditOrderDialog в DialogService

Добавить мягкое удаление (IsDeleted)

Всем:

bash
# Обязательные обновления
git pull origin feature/viewmodels
npm install # для фронтенд-части


Для отладки VM без UI:

csharp
// В Program.cs (временный файл)
var vm = new CustomersViewModel(dbService, dialogService);
vm.AddCommand.Execute(null);
Тестирование фильтрации заказов:

csharp
OrdersVM.SelectedStatus = OrderStatus.Completed;
OrdersVM.ApplyFilters(); // Должны отфильтроваться заказы
Шаблон для новых VM:

csharp
public class NewViewModel : ViewModelBase
{
    private readonly IDependency _dep;
    public ICommand ActionCommand { get; }
    
    public NewViewModel(IDependency dep)
    {
        _dep = dep;
        ActionCommand = new RelayCommand(ExecuteAction);
    }
    
    private void ExecuteAction(object param)
    {
        // Логика
    }
}
