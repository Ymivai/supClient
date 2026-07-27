# SupClient — architecture (current)

Локальное MAUI-приложение MVP: бронирования SUP-досок, проверка доступности, настройки. Диаграммы: [Mermaid](https://mermaid.js.org/).

**Кратко:** **`MauiProgram`** регистрирует сервисы и страницы в DI. **`AppShell`** — TabBar (Брони / Настройки). **`BookingsPage`** показывает брони текущего дня; **`AddBookingPage`** — push поверх таба. **`BookingAvailabilityService`** — вся логика пересечений и свободных досок. **`JsonBookingRepository`** и **`AppSettingsService`** — JSON в `FileSystem.AppDataDirectory`. ViewModels общаются через **`INavigationService`**, **`IDialogService`**, **`WeakReferenceMessenger`**.

## Class diagram

```mermaid
classDiagram
    direction TB

    class MauiProgram {
        <<composition root>>
        RegisterServices()
    }

    class App {
        +CreateWindow()
    }

    class AppShell {
        TabBar Bookings / Settings
        RegisterRoute AddBookingPage
    }

    class IBookingRepository {
        <<interface>>
        +GetBookingsByDateAsync()
        +AddBookingAsync()
        +DeleteBookingAsync()
        +UpdateBookingAsync()
    }

    class JsonBookingRepository {
        bookings.json
    }

    class IAppSettingsService {
        <<interface>>
        +GetSettingsAsync()
        +SaveSettingsAsync()
    }

    class AppSettingsService {
        settings.json
    }

    class IBookingAvailabilityService {
        <<interface>>
        +CheckAvailabilityAsync()
        +GetAvailableBoardsAt()
        +FindNextAvailableStart()
    }

    class BookingAvailabilityService {
        peak occupancy sweep
    }

    class INavigationService {
        <<interface>>
        +NavigateToPage()
        +NavigateBack()
    }

    class NavigationService {
        VM lifecycle hooks
    }

    class IDialogService {
        <<interface>>
        +DisplayAlertAsync()
    }

    class BookingsPageViewModel
    class AddBookingPageViewModel
    class SettingsPageViewModel
    class ViewModelBase

    class BookingsPage
    class AddBookingPage
    class SettingsPage

    class Booking {
        Id StartTime Duration BoardsCount
        EndTime computed
    }

    class AppSettings {
        TotalBoards
    }

    MauiProgram ..> IBookingRepository : Singleton
    MauiProgram ..> IAppSettingsService : Singleton
    MauiProgram ..> IBookingAvailabilityService : Singleton
    MauiProgram ..> INavigationService : Singleton
    MauiProgram ..> IDialogService : Singleton
    MauiProgram ..> BookingsPage : Transient
    MauiProgram ..> AddBookingPage : Transient

    App --> AppShell
    AppShell --> BookingsPage
    AppShell --> SettingsPage

    IBookingRepository <|.. JsonBookingRepository
    IAppSettingsService <|.. AppSettingsService
    IBookingAvailabilityService <|.. BookingAvailabilityService
    INavigationService <|.. NavigationService

    BookingAvailabilityService --> IBookingRepository
    BookingAvailabilityService --> IAppSettingsService

    BookingsPageViewModel --> IBookingRepository
    BookingsPageViewModel --> INavigationService
    AddBookingPageViewModel --> IBookingRepository
    AddBookingPageViewModel --> IBookingAvailabilityService
    AddBookingPageViewModel --> INavigationService
    AddBookingPageViewModel --> IDialogService
    SettingsPageViewModel --> IAppSettingsService
    SettingsPageViewModel --> IDialogService

    ViewModelBase <|-- BookingsPageViewModel
    ViewModelBase <|-- AddBookingPageViewModel
    ViewModelBase <|-- SettingsPageViewModel

    BookingsPage --> BookingsPageViewModel : BindingContext
    AddBookingPage --> AddBookingPageViewModel : BindingContext
    SettingsPage --> SettingsPageViewModel : BindingContext

    JsonBookingRepository ..> Booking : persists
    AppSettingsService ..> AppSettings : persists
```

## Flow: добавление брони

```mermaid
sequenceDiagram
    participant UI as BookingsPage
    participant BVM as BookingsPageViewModel
    participant Nav as NavigationService
    participant Add as AddBookingPageViewModel
    participant Av as BookingAvailabilityService
    participant Repo as JsonBookingRepository
    participant Dialog as DialogService
    participant Msg as WeakReferenceMessenger

    UI->>BVM: AddBookingCommand
    BVM->>Nav: NavigateToPage AddBookingPage(date)
    Nav->>Add: OnNavigatingTo(date)
    Add->>Add: user sets time + boards
    Add->>Av: CheckAvailabilityAsync(start, count)
    Av->>Repo: GetBookingsByDateAsync
    alt достаточно досок
        Add->>Repo: AddBookingAsync(booking)
        Add->>Msg: BookingsChangedMessage
        Add->>Nav: NavigateBack()
        Nav->>BVM: OnReturnedTo / reload
    else недостаточно
        Av-->>Add: AvailabilityCheckResult
        Add->>Dialog: DisplayAlert(message)
    end
```

## Flow: проверка доступности

```mermaid
sequenceDiagram
    participant Av as BookingAvailabilityService
    participant Settings as AppSettingsService
    participant Repo as JsonBookingRepository

    Av->>Settings: GetSettingsAsync → TotalBoards
    Av->>Repo: GetBookingsByDateAsync(date)
    Av->>Av: GetPeakOccupancy(window)
    Note over Av: available = TotalBoards - peak
    alt available >= requested
        Av-->>Av: IsAvailable = true
    else
        Av->>Av: FindNextAvailableStart(candidates)
        Av-->>Av: NextAvailableStart
    end
```

## Component roles

| Component | Role |
|-----------|------|
| **MauiProgram** | Composition root: CommunityToolkit, logging, регистрация Singleton/Transient. |
| **AppShell** | TabBar; страницы инжектятся из DI (не `DataTemplate`); маршрут `AddBookingPage`. |
| **ViewModelBase** | `INotifyPropertyChanged`, lifecycle: `OnNavigatingTo`, `OnNavigatedTo`, `OnReturnedTo`, `OnClosing`. |
| **JsonBookingRepository** | CRUD броней в `bookings.json`, фильтр по дате, сортировка по `StartTime`. |
| **AppSettingsService** | Чтение/запись `settings.json` (`TotalBoards`). |
| **BookingAvailabilityService** | Peak occupancy, свободные доски, ближайший слот; без UI. |
| **NavigationService** | Push/Pop + вызов lifecycle ViewModel. |
| **DialogService** | Alert через текущую `Page`. |
| **BookingsChangedMessage** | Обновление списка после добавления (Messenger). |

## Слои и зависимости

```
Views  →  ViewModels  →  Services / Storage (interfaces)
                ↓
         Models, Messages, Defines
```

- View **не** вызывает Repository напрямую.
- **BookingAvailabilityService** — единственное место алгоритма пересечений (позже — вызов API с тем же контрактом).

## Файлы данных

| Файл | Путь | Содержимое |
|------|------|------------|
| `bookings.json` | `FileSystem.AppDataDirectory` | Массив `Booking` |
| `settings.json` | `FileSystem.AppDataDirectory` | `AppSettings` |

## Notes

- Ц целевой платформы: **net9.0-android**, **net9.0-ios**; Windows TFM — для разработки на ПК.
- Длительность брони фиксирована: `Defines.DefaultBookingDuration` = 2 часа; модель `Booking.Duration` уже позволяет менять позже.
- См. также: [development-plan.md](./development-plan.md), [modules/Availability/AvailabilitySpecification.md](./modules/Availability/AvailabilitySpecification.md).

## Target architecture (будущее)

При появлении сервера ожидаемая схема (без переписывания UI):

```mermaid
classDiagram
    class IBookingRepository {
        <<interface unchanged>>
    }
    class ApiBookingRepository {
        HTTP + local cache
    }
    class BookingAvailabilityService {
        local or remote strategy
    }

    IBookingRepository <|.. JsonBookingRepository : MVP
    IBookingRepository <|.. ApiBookingRepository : future
    BookingAvailabilityService --> IBookingRepository
```

Репозиторий и `IBookingAvailabilityService` остаются контрактами; реализация меняется или дополняется sync-слоем.
