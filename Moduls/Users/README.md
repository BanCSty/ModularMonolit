# Users Module

Модуль управления пользователями в системе.

## 🎯 Назначение

Модуль отвечает за:
- Жизненный цикл пользователей (CRUD)
- Хранение агрегированной информации о заказах пользователя (`UserOrderSummary`)
- Публикацию доменных событий для других модулей

## 🏗️ Архитектура
```
Users/
├── Users.Domain/ # Доменные сущности
│ ├── Entities/
│ │ ├── User.cs
│ │ └── UserOrderSummary.cs # Read Model
│ └── Repositories/
│ ├── IUserRepository.cs
│ └── IUserOrderSummaryRepository.cs
│
├── Users.Application/ # Бизнес-логика
│ ├── Services/
│ │ └── UserService.cs
│ └── EventHandlers/
│ ├── OrderCreatedEventHandler.cs
│ ├── OrderUpdatedEventHandler.cs
│ └── OrderDeletedEventHandler.cs
│
├── Users.Infrastructure/ # Инфраструктура
│ ├── Persistence/
│ │ ├── UserDbContext.cs
│ │ ├── Migrations/
│ │ └── Repositories/
│ └── Outbox/
│
└── Users.Presentation/
├── Controllers/
├── Contracts/
└── UsersModule.cs
```

## 💾 База данных

SQLite, путь src\ModularMonolit\data\users.db (для удобства)

**Таблицы:**

### `Users` — основная информация
| Поле | Тип | Описание |
|------|-----|----------|
| Id | int | PK |
| Name | string | Имя |
| Email | string | Email |
| CreatedAt | DateTime | Дата создания |
| UpdatedAt | DateTime? | Дата обновления |

### `UserOrderSummaries` — Read Model
| Поле | Тип | Описание |
|------|-----|----------|
| UserId | int | PK, FK на Users |
| TotalOrders | int | Количество заказов |
| TotalSpent | decimal | Общая сумма заказов |
| LastOrderDate | DateTime? | Дата последнего заказа |
| UpdatedAt | DateTime | Дата обновления записи |

### `OutboxMessages` — исходящие события

## Паттерн Materialized View

`UserOrderSummary` — это **локальная копия агрегированных данных** о заказах пользователя.

### Зачем это нужно?

**Без дублирования (медленно):**
1. Производительность: нет сетевых вызовов к другим модулям
2. Независимость: модуль Users работает даже если Orders недоступен
3. Оптимизация: данные уже агрегированы
4. Готовность к микросервисам: после распила не нужно менять код

## Обрабатываемые события
Модуль подписан на события из модуля Orders:
```
     Событие	              Обработчик	                  Действие
OrderCreatedEvent	OrderCreatedEventHandler	Увеличить TotalOrders, TotalSpent, обновить LastOrderDate
OrderUpdatedEvent	OrderUpdatedEventHandler	Пересчитать TotalSpent
OrderDeletedEvent	OrderDeletedEventHandler	Уменьшить TotalOrders, TotalSpent
```
## API Endpoints
```
Метод	URL	Описание
GET	/api/users	Получить всех пользователей
GET	/api/users/{id}	Получить пользователя
GET	/api/users/{id}/summary	Получить сводку по заказам
POST	/api/users	Создать пользователя
PUT	/api/users/{id}	Обновить пользователя
DELETE	/api/users/{id}	Удалить пользователя
```

## Взаимодействие с другими модулями - пример
```
┌─────────────────────────────────────────────┐
│                                             │
│  Users Module                               │
│  ┌─────────────┐                            │
│  │ UserService │─── publishes ──┐           │
│  └─────────────┘                │           │
│                                 ▼           │
│                          UserCreatedEvent   │
│                                 │           │
└─────────────────────────────────┼───────────┘
                                  │
                                  ▼
                          ┌───────────────┐
                          │ Orders Module │
                          │               │
                          │ UserCreated   │
                          │ EventHandler  │
                          └───────────────┘
```

## Пример: Обновление сводки 
1. Клиент создает заказ через Orders API
        ↓
2. Orders модуль сохраняет заказ + OrderCreatedEvent в Outbox (в одной транзакции)
        ↓
3. OutboxPublisher в Orders публикует событие
        ↓
4. EventBus доставляет событие в Users модуль
        ↓
5. OrderCreatedEventHandler обновляет UserOrderSummary
        ↓
6. Клиент запрашивает /api/users/{id}/summary и видит актуальные данные

## Примечания
- Источник истины для заказов — модуль Orders
- UserOrderSummary — производные данные (Read Model)
- Возможна временная рассогласованность (eventual consistency) — данные обновляются с задержкой ~2 секунды
- При выделении в микросервис потребуется заменить InMemory EventBus на RabbitMQ