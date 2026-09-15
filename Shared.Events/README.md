# Shared.Events

Библиотека для работы с событиями: определения событий и реализация EventBus.

## 🎯 Назначение

- **Контракты событий** — общие доменные события между модулями
- **InMemory EventBus** — реализация шины событий для монолита
- **Базовые классы** — для упрощения создания событий

## 📦 Содержимое

### `InMemoryEventBus`

Реализация `IEventBus` для взаимодействия модулей **внутри одного процесса**.

```csharp
public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Type>> _handlerTypes = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryEventBus> _logger;
    
    public void Subscribe<TEvent, THandler>() { ... }
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct) { ... }
    public async Task PublishAsync(object @event, Type eventType, CancellationToken ct) { ... }
}
```
## Ключевые особенности
- Хранит только Type — не создает утечек памяти
- Создает scope для каждого обработчика — изолирует зависимости
- Использует reflection для вызова HandleAsync (для не-generic случая)

## Как работает
```
[PublishAsync(@event)]
        ↓
[Находим подписчиков по типу события]
        ↓
[Для каждого обработчика:]
   ├─ Создаем scope
   ├─ Получаем handler из DI
   ├─ Вызываем HandleAsync через reflection
   └─ Уничтожаем scope
```

# Регистрация
```
// Program.cs
//Обязательно Singleton Иначе подписки будут теряться.
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
```

## Схема подписок
```
[Users Module]                          [Orders Module]
     │                                       │
UserService                          OrderService
     │                                       │
     ├─ publishes UserCreatedEvent           ├─ publishes OrderCreatedEvent
     │                                       │
     ▼                                       ▼
┌─────────────────────────────────────────────────────┐
│                  InMemoryEventBus                   │
│                                                     │
│  UserCreatedEvent  → [UserCreatedEventHandler]      │
│  UserUpdatedEvent  → [UserUpdatedEventHandler]      │
│  OrderCreatedEvent → [OrderCreatedEventHandler]     │
│  OrderUpdatedEvent → [OrderUpdatedEventHandler]     │
└─────────────────────────────────────────────────────┘
```

## Поток события
1. Бизнес-операция завершена
        ↓
2. Событие сохранено в Outbox (в транзакции с бизнес-данными)
        ↓
3. OutboxPublisher находит событие
        ↓
4. OutboxPublisher десериализует событие
        ↓
5. EventBus.PublishAsync(@event, eventType)
        ↓
6. EventBus находит подписчиков по типу
        ↓
7. Для каждого подписчика создается scope
        ↓
8. Handler.HandleAsync(@event)

## Подготовка к микросервисам
```
// InMemoryEventBus (сейчас)
services.AddSingleton<IEventBus, InMemoryEventBus>();

// RabbitMQEventBus (после распила)
services.AddSingleton<IEventBus, RabbitMQEventBus>();
```

## Примечания
- InMemoryEventBus работает только внутри одного процесса
- Для нескольких процессов не подходит
- Гарантирует at-least-once доставку (через Outbox)
- Не гарантирует порядок обработки