# Modular Monolith with Outbox Pattern

## 📝 Описание проекта

Это учебный проект, демонстрирующий архитектуру **модульного монолита** с подготовкой к будущему разделению на микросервисы. Проект показывает, как организовать модульную структуру, настроить взаимодействие между модулями и реализовать паттерн **Transactional Outbox** для надежной публикации событий.

## 🎯 Цели проекта

- Изучение принципов модульного монолита
- Понимание паттерна Transactional Outbox
- Подготовка к миграции на микросервисную архитектуру
- Практика работы с несколькими DbContext
- Освоение event-driven архитектуры

## 🏗️ Архитектура

### Модульная структура
```
ModularMonolit/
├── src/
│ ├── ModularMonolit.Api/ # Startup проект
│ ├── Users/ # Модуль пользователей
│ │ ├── Users.Domain/ # Доменные сущности
│ │ ├── Users.Application/ # Бизнес-логика
│ │ ├── Users.Infrastructure/ # Доступ к данным
│ │ └── Users.Presentation/ # контроллеры
│ │
│ ├── Orders/ # Модуль заказов
│ │ ├── Orders.Domain/
│ │ ├── Orders.Application/
│ │ ├── Orders.Infrastructure/
│ │ └── Orders.Presentation/
│ │
│ └── Shared/ # Общие компоненты
│ ├── Shared.Abstractions/ # Интерфейсы
│ ├── Shared.Events/ # Контракты событий
│ └── Shared.Outbox/ # Outbox паттерн
```

### Взаимодействие модулей
```
[Users Module] [Orders Module]
│ │
│ База данных users.db │ База данных orders.db
│ ├── Users │ ├── Orders
│ └── OutboxMessages │ └── OutboxMessages
│ │
└────────── Event Bus ─────────────┘
```

## 🔄 Паттерн Transactional Outbox

### Как это работает

1. **Бизнес-операция** выполняется в транзакции
2. **Событие сохраняется** в таблицу OutboxMessages в той же транзакции
3. **Фоновый сервис** (OutboxPublisher) отслеживает новые сообщения
4. **Событие публикуется** в Event Bus
5. **Обработчики** в других модулях реагируют на событие


## 📦 Реализованные модули

### Users Module
- **Функциональность**: CRUD операции с пользователями
- **База данных**: SQLite (users.db)
- **События**: UserCreatedEvent, UserUpdatedEvent, UserDeletedEvent

### Orders Module
- **Функциональность**: Управление заказами
- **База данных**: SQLite (orders.db)
- **События**: OrderCreatedEvent, OrderUpdatedEvent, OrderCancelledEvent
- **Обработчики**: Реагирует на события пользователей

## 🛠️ Технологии

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **SQLite**
- **MediatR** (для CQRS)
- **FluentValidation**
- **InMemory Event Bus** (собственная реализация)

## 🚀 Запуск проекта

### Предварительные требования

- .NET 8.0 SDK
- Visual Studio 2022 или VS Code

### Установка и запуск

```bash
# Клонировать репозиторий
git clone ...

# Перейти в директорию проекта
cd ModularMonolit

# Восстановить зависимости
dotnet restore

# Применить миграции (автоматически при запуске)
dotnet run --project src/ModularMonolit.Api

# Для Users модуля
dotnet ef migrations add MigrationName \
    --project src/Users.Infrastructure \
    --startup-project src/ModularMonolit.Api \
    --output-dir Persistence/Migrations

# Для Orders модуля
dotnet ef migrations add MigrationName \
    --project src/Orders.Infrastructure \
    --startup-project src/ModularMonolit.Api \
    --output-dir Persistence/Migrations
```