# Shared.Abstractions

Библиотека **чистых абстракций** — интерфейсов и контрактов, которые используются во всех модулях.

## 🎯 Назначение

Определяет **публичный контракт** для:
- Модулей (`IModule`)
- Работы с транзакциями (`IUnitOfWork`)
- Публикации событий (`IEventBus`)
- Обработки событий (`IEventHandler<T>`)
- Работы с Outbox (`IOutboxService<TContext>`)

## ⚠️ Принципы

- ❌ **Нет зависимостей** на другие Shared проекты
- ❌ **Нет зависимостей** на конкретные модули
- ✅ Только **контракты**

## 📦 Содержимое

### `IModule`

Контракт для модуля Modular Monolith.

```csharp
public interface IModule
{
    string Name { get; }
    
    /// Регистрация сервисов модуля в DI
    IServiceCollection RegisterModule(
        IServiceCollection services, 
        IConfiguration configuration);
    
    /// Подписка на события других модулей
    void ConfigureEventSubscriptions(IEventBus eventBus);
    
    /// Маппинг эндпоинтов модуля
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
```
## Диаграмма зависимостей
```
┌──────────────────────────────────────────┐
│         Shared.Abstractions              │
│                                          │
│  IModule  IUnitOfWork  IEventBus         │
│  IEventHandler<T>  IOutboxService<T>     │
└──────────────────────────────────────────┘
              ▲
              │ use
              │
    ┌─────────┴─────────┐
    │                   │
[Users Module]    [Orders Module]
```
## Примечания
* Проект не имеет внешних зависимостей (кроме Microsoft.Extensions.DependencyInjection.Abstractions)
* Все интерфейсы минималистичны — только необходимое
* При выделении в микросервис проекты становятся NuGet-пакетом