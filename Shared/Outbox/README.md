# Shared.Outbox

Реализация паттерна **Transactional Outbox** для гарантированной публикации событий.

## 🎯 Назначение

Обеспечивает **атомарность** между сохранением бизнес-данных и публикацией событий:

- ✅ Либо **всё сохранится** (данные + событие)
- ✅ Либо **ничего** (при ошибке — откат транзакции)
- ✅ Событие **не потеряется** при сбое приложения
- ✅ Доставка "at-least-once"

## 🐛 Проблема, которую решает

### Без Outbox (плохо)

```csharp
// Внутри транзакции
await _orderRepository.AddAsync(order);
await _unitOfWork.SaveChangesAsync();
await _unitOfWork.CommitTransactionAsync();

// ❌ ПРОБЛЕМА: если тут приложение упадет — событие потеряно
await _eventBus.PublishAsync(new OrderCreatedEvent(...));
```
### С Outbox (хорошо)
```csharp
await _unitOfWork.BeginTransactionAsync();

await _orderRepository.AddAsync(order);
await _outboxService.AddEventAsync(new OrderCreatedEvent(...)); // ← в одной транзакции!

await _unitOfWork.SaveChangesAsync();
await _unitOfWork.CommitTransactionAsync();
```
Если приложение упадет после Commit — событие всё равно в БД. Background service его опубликует.

## Сущность для хранения события в БД.
```
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }        // Полное имя типа с assembly
    public string Payload { get; private set; }     // JSON
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }
    
    public void MarkAsPublished();
    public void MarkAsFailed(string error);
}
```

## ⚠️ Почему generic <TContext>?
Это критически важно для модульного монолита! Каждый модуль имеет свой IOutboxService<TContext>:
```
// Users module
services.AddScoped<IOutboxService<UserDbContext>, OutboxService<UserDbContext>>();

// Orders module
services.AddScoped<IOutboxService<OrderDbContext>, OutboxService<OrderDbContext>>();
```

## Без generic было бы:
❌ DI-контейнер возвращает последнюю зарегистрированную реализацию
❌ Users получил бы OutboxService<OrderDbContext>
❌ События сохранялись бы в чужую БД

## Полный цикл работы
```
┌───────────────────────────────────────────────────────────┐
│                    HTTP Request                           │
└───────────────────────┬───────────────────────────────────┘
                        ▼
┌───────────────────────────────────────────────────────────┐
│  BEGIN TRANSACTION                                        │
├───────────────────────────────────────────────────────────┤
│  1. Save Order to Orders table                            │
│  2. INSERT INTO OutboxMessages (OrderCreatedEvent)        │
├───────────────────────────────────────────────────────────┤
│  COMMIT                                                   │
└───────────────────────┬───────────────────────────────────┘
                        ▼
               [Response to Client]
                        
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    2 секунды спустя
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

┌───────────────────────────────────────────────────────────┐
│  OutboxPublisher (Background Service)                     │
├───────────────────────────────────────────────────────────┤
│  1. SELECT * FROM OutboxMessages WHERE PublishedAt IS NULL│
│  2. Deserialize OrderCreatedEvent                         │
│  3. EventBus.PublishAsync(@event)                         │
│  4. UPDATE OutboxMessages SET PublishedAt = NOW()         │
└───────────────────────┬───────────────────────────────────┘
                        ▼
┌───────────────────────────────────────────────────────────┐
│  EventBus → OrderCreatedEventHandler (в Users)            │
│  → Обновляем UserOrderSummary                             │
└───────────────────────────────────────────────────────────┘
```
## Регистрация сервисов
```
// В модуле
services.AddDbContext<OrderDbContext>(...);
services.AddScoped<IOutboxService<OrderDbContext>, OutboxService<OrderDbContext>>();
services.AddHostedService<OutboxPublisher<OrderDbContext>>();
```

## Обработка ошибок
### Retry политика
```
.Where(m => m.PublishedAt == null && m.RetryCount < 5)
```
Максимум 5 попыток. После — сообщение игнорируется.

## Подготовка к микросервисам
```
[Mikroservice A]                    [Mikroservice B]
     │                                    │
  Outbox Table                       Consumer
     │                                    ▲
     ▼                                    │
OutboxPublisher → RabbitMQ Queue ────────┘
```
### Что изменится:
- EventBus.PublishAsync → RabbitMQ.PublishAsync

### Что НЕ изменится:
- Схема таблицы Outbox
- Логика OutboxService
- Логика OutboxPublisher