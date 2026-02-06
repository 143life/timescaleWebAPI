# Timescale Web API

WebAPI приложение для работы с временными рядами данных с использованием TimescaleDB.

## Функциональность

### 1. Загрузка CSV файлов
- Формат: `Date;ExecutionTime;Value`
- Пример: `2024-01-01T10-00-00.0000Z;1.5;100.0`
- Валидация:
  - Дата: от 01.01.2000 до текущей даты
  - ExecutionTime ≥ 0
  - Value ≥ 0
  - Количество строк: 1-10,000
- Автоматический расчет статистики
- Перезапись при повторной загрузке

### 2. Фильтрация результатов
- По имени файла
- По диапазону даты запуска
- По диапазону среднего значения
- По диапазону среднего времени выполнения
- Пагинация

### 3. Получение последних значений
- 10 последних записей для указанного файла
- Сортировка по дате (по убыванию)

## Технологии
- .NET 9.0
- Entity Framework Core
- PostgreSQL
- Swagger/OpenAPI
- Docker (планируется)

## Запуск без Docker

### Требования
- .NET 9.0 SDK
- PostgreSQL

### Шаги
1. Клонируйте репозиторий:
```bash
git clone <url>
cd TimescaleWebAPI
```

2. Создайте БД
```bash
# Подключение к БД
psql -U postgres
# Создание БД
CREATE DATABASE cleantimescaledb;
```

3. Настройте подключение к БД в src/TimescaleWebAPI.API/appsettings.json
(и в src/TimescaleWebAPI.API/appsettings.Development.json):
```bash
# Для удобства в тестовом проекте эти файлы не добавлены в .gitignore
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cleantimescaledb;Username=your_username;Password=your_password"
  }
}
```

4. Примените миграции:
```bash
cd src/TimescaleWebAPI.API
# Если команда выдаст ошибку, попробуйте запустить ее два раза подряд
dotnet ef database update --project src/TimescaleWebAPI.Infrastructure --startup-project src/TimescaleWebAPI.API
```

5. Запустите приложение:
```bash
dotnet run --project src/TimescaleWebAPI.API
```

6. Откройте Swagger UI: http://localhost:5011/swagger

## Тестирование

### Unit-тесты

### Запуск тестов

```bash
# Запуск всех unit-тестов
dotnet test tests/TimescaleWebAPI.UnitTests

# Запуск класса тестов
dotnet test --filter "ValueTests"
dotnet test --filter "CsvValidatorTests"

# Запуск с детальным выводом
dotnet test --verbosity normal
```