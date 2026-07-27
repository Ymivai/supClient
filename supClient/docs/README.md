# SupClient — документация

Мобильное приложение для учёта бронирований SUP-досок (Android / iOS, .NET MAUI).

Архитектура и стиль документов ориентированы на [Hideez-Mobile-Client](../../hideez-mobile-clien): краткое резюме, Mermaid-диаграммы, спецификации модулей, пошаговые flow.

## Содержание

| Документ | Описание |
|----------|----------|
| [architecture-current.md](./architecture-current.md) | Текущая архитектура MVP: слои, DI, диаграммы классов и flow |
| [development-plan.md](./development-plan.md) | План разработки: этапы, статус, что делать дальше |
| [modules/Bookings/BookingsSpecification.md](./modules/Bookings/BookingsSpecification.md) | Модель брони, экраны, добавление |
| [modules/Availability/AvailabilitySpecification.md](./modules/Availability/AvailabilitySpecification.md) | Проверка доступности, peak occupancy, ближайшее время |
| [modules/Storage/StorageSpecification.md](./modules/Storage/StorageSpecification.md) | Локальное хранилище, репозитории, файлы данных |
| [modules/Settings/SettingsSpecification.md](./modules/Settings/SettingsSpecification.md) | Настройки приложения (TotalBoards и расширение) |
| [modules/Navigation/NavigationSpecification.md](./modules/Navigation/NavigationSpecification.md) | Навигация, lifecycle ViewModel |
| [../TROUBLESHOOTING.md](../TROUBLESHOOTING.md) | Сборка, типичные проблемы |

## Быстрый старт

```powershell
cd E:\source\repos\supClient
dotnet build supClient.sln -f net9.0-android
# Windows (для быстрой проверки UI):
dotnet build supClient.sln -f net9.0-windows10.0.19041.0
```

## Структура проекта

```
supClient/
├── Models/           # Booking, AppSettings
├── ViewModels/       # MVVM, ViewModelBase
├── Views/            # XAML-страницы (без бизнес-логики)
├── Services/         # Availability, Navigation, Dialog
├── Storage/          # IBookingRepository, IAppSettingsService
├── Messages/         # WeakReferenceMessenger
├── Defines.cs        # Константы приложения
├── MauiProgram.cs    # DI, регистрация сервисов
└── docs/             # Эта документация
```

## Принципы (как в Hideez)

- **MVVM** — логика в ViewModel и Services, code-behind только инициализация и `OnAppearing`.
- **DI** — все зависимости через `MauiProgram.RegisterServices`.
- **Интерфейсы хранилища** — ViewModel не знает про JSON/файлы; позже можно подменить на API.
- **Бизнес-логика в сервисе** — `BookingAvailabilityService` не зависит от UI; в будущем тот же контракт на сервере.
- **Спецификации рядом с модулем** — при добавлении фичи создавать или обновлять `docs/modules/<Module>/`.

## Статус MVP

| Область | Статус |
|---------|--------|
| Локальное хранение | ✅ |
| Список броней за день | ✅ |
| Добавление брони | ✅ |
| Проверка пересечений | ✅ |
| Расчёт свободных SUP | ✅ |
| Ближайшее доступное время | ✅ |
| Настройки TotalBoards | ✅ |
| Выбор даты | ⏳ план |
| Редактирование / удаление | ⏳ план |
| Сервер / синхронизация | ⏳ не в scope MVP |

Подробнее — [development-plan.md](./development-plan.md).
