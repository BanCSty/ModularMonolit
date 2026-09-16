# Orders Module

Модуль управления заказами в системе.

## 🎯 Назначение

Модуль отвечает за жизненный цикл заказов:
- Создание заказов
- Обновление статуса и данных заказа
- Удаление заказов
- Публикация доменных событий для других модулей

## 🏗️ Архитектура

Модуль построен по принципам Clean Architecture:
```
Orders/
├── Orders.Domain/ # Доменные сущности и правила
│ ├── Entities/
│ │ └── Order.cs
│ └── Repositories/
│ └── IOrderRepository.cs
│
├── Orders.Application/ # Бизнес-логика
│ ├── Services/
│ │ └── OrderService.cs
│ └── EventHandlers/
│ ├── UserCreatedEventHandler.cs
│ └── UserUpdatedEventHandler.cs
│
├── Orders.Infrastructure/ # Инфраструктура
│ ├── Persistence/
│ │ ├── OrderDbContext.cs
│ │ ├── Migrations/
│ │ └── Repositories/
│ └── Outbox/ # См. Shared.Outbox
│
└── Orders.Presentation/ # API
├── Controllers/
├── Contracts/
└── OrdersModule.cs # Регистрация модуля
```


## 💾 База данных

SQLite, находится в src\ModularMonolit\data\orders.db 

**Таблицы:**
- `Orders` — заказы
- `OutboxMessages` — исходящие события (Transactional Outbox)

## 📤 Публикуемые события

Модуль публикует следующие события через Outbox Pattern:
```
| Событие | Когда публикуется |
|---------|-------------------|
| `OrderCreatedEvent` | При создании заказа |
| `OrderUpdatedEvent` | При обновлении заказа |
| `OrderDeletedEvent` | При удалении заказа |
```
### Пример события

```csharp
public record OrderCreatedEvent
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### API Endpoints
```
GET	/api/orders	            Получить все заказы
GET	/api/orders/{id}	    Получить заказ по ID
POST	/api/orders	        Создать заказ
PUT	/api/orders/{id}	    Обновить заказ
DELETE	/api/orders/{id}    Удалить заказ
```

### Паттерн Transactional Outbox
Все операции записи используют Outbox Pattern для гарантированной доставки событий:
```
public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
{
    await _unitOfWork.BeginTransactionAsync();
    
    // 1. Создаем заказ
    var order = new Order(dto.UserId, dto.Total);
    await _orderRepository.AddAsync(order);
    
    // 2. Сохраняем событие в Outbox (в той же транзакции)
    await _outboxService.AddEventAsync(new OrderCreatedEvent(
        order.Id, order.UserId, order.Total, order.CreatedAt));
    
    // 3. Коммитим — либо все, либо ничего
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
    
    return MapToDto(order);
}
```

### Взаимодействие с другими модулями
```
┌─────────────────────────────────────────────┐
│                                             │
│  Orders Module                              │
│  ┌─────────────┐                            │
│  │ OrderService│─── publishes ──┐           │
│  └─────────────┘                │           │
│                                 ▼           │
│                          OrderCreatedEvent  │
│                                 │           │
└─────────────────────────────────┼───────────┘
                                  │
                                  ▼
                          ┌───────────────┐
                          │ Users Module  │
                          │               │
                          │ OrderCreated  │
                          │ EventHandler  │
                          │       │       │
                          │       ▼       │
                          │ UserOrder     │
                          │ Summary       │
                          └───────────────┘
```
### Примечания
- Модуль не знает о существовании Users модуля напрямую
- Общение только через события
- Готов к выделению в отдельный микросервис (нужно только заменить InMemory EventBus на RabbitMQ)