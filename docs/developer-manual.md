# Руководство программиста — GeekTour

## Стек технологий

| Компонент | Технология |
|-----------|-----------|
| Язык | C# 12.0 |
| Фреймворк | ASP.NET Core MVC 8.0 |
| ORM | Entity Framework Core 8.0 |
| База данных | SQLite |
| ML | ML.NET 3.0 (Matrix Factorization) |
| UI | Razor Views + Bootstrap 5 + Font Awesome 6 |
| Карты | Leaflet.js + OpenStreetMap |
| Экспорт | ClosedXML (Excel), DocumentFormat.OpenWord (Word) |

---

## Структура решения

```
GeekTour/
├── GeekTour.sln                      # Файл решения
├── GeekTour.Web/                     # Основной проект
│   ├── Controllers/                  # Контроллеры MVC
│   ├── Models/
│   │   ├── Entities/                 # Сущности EF Core
│   │   ├── ViewModels/               # Модели представлений
│   │   └── Enums/                    # Перечисления
│   ├── Data/
│   │   ├── AppDbContext.cs           # Контекст БД
│   │   ├── DbInitializer.cs          # Инициализация тестовых данных
│   │   └── Configurations/           # Fluent API конфигурации
│   ├── Services/                     # Бизнес-логика
│   ├── ML/                           # ML.NET модели
│   ├── Views/                        # Razor-представления
│   ├── wwwroot/
│   │   ├── css/                      # Стили
│   │   ├── js/                       # Скрипты
│   │   └── uploads/                  # Загруженные файлы
│   └── Program.cs                    # Точка входа, DI
├── GeekTour.Tests/                   # Тесты (xUnit)
└── docs/                             # Документация
```

---

## Архитектура

### Паттерн MVC

Приложение следует паттерну Model-View-Controller:
- **Models** — сущности БД + ViewModels для представлений
- **Views** — Razor-шаблоны с Bootstrap 5
- **Controllers** — обработка HTTP-запросов, вызов сервисов

### Сервисный слой

Бизнес-логика вынесена в сервисы:

| Сервис | Ответственность |
|--------|----------------|
| `IAuthService` | Регистрация, авторизация |
| `ILocationService` | CRUD локаций, фильтрация, рекомендации |
| `IReviewService` | Отзывы, модерация, ответы |
| `IRouteService` | Маршруты, шеринг |
| `IRouteOptimizer` | Алгоритм оптимизации (NN + 2-opt) |
| `IReportService` | Генерация статистики |
| `IExportService` | Экспорт в Excel/Word |
| `IFileStorageService` | Сохранение загруженных файлов |

### Dependency Injection

Все сервисы регистрируются в `Program.cs`:
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IRouteOptimizer, RouteOptimizer>();
// ...
```

---

## Модель данных

### Ключевые сущности

```
User (пользователь)
├── Id, Email, PasswordHash, Name, Role
├── CompanyName, INN, IsVerified (для партнёров)
├── Reviews → List<Review>
├── Routes → List<TourRoute>
└── OwnedLocations → List<Location>

Location (заведение)
├── Id, Name, Description, Type
├── Address, Latitude, Longitude
├── WorkingHoursJson (JSON расписание)
├── CategoryId → Category
├── OwnerId → User (партнёр)
├── LocationFandoms → many-to-many с Fandom
├── Reviews, Images, Promotions, Events, MenuCatalog, ViewStats

TourRoute (маршрут)
├── Id, UserId, Name, Description
├── CreatedAt, IsPublic, ShareLink
├── TotalDistanceKm, EstimatedTimeMinutes
└── Points → List<RoutePoint>

Review (отзыв)
├── Id, UserId, LocationId
├── Rating (1-5), Text, PhotoPaths
├── CreatedAt, IsModerated
└── OwnerResponse (ответ владельца)

Fandom (тематика)
├── Id, Name, Category (enum: Anime, Comics, VideoGames, BoardGames)
└── LocationFandoms → many-to-many с Location
```

---

## Алгоритм оптимизации маршрутов

Реализован в `RouteOptimizer.cs`:

### Фаза 1: Nearest Neighbor
1. Начинаем с точки с наивысшим рейтингом
2. На каждом шаге выбираем ближайшую точку с учётом весовых коэффициентов:
   ```
   score = DistanceWeight × distance − RatingWeight × avgRating
   ```

### Фаза 2: 2-opt Improvement
1. Перебираем все пары рёбер маршрута
2. Если перестановка уменьшает общую дистанцию — применяем
3. Повторяем до отсутствия улучшений

### Весовые коэффициенты
Настраиваются администратором:
- `DistanceWeight` = 0.4 (по умолчанию)
- `TimeWeight` = 0.3
- `RatingWeight` = 0.3

### Расчёт расстояния
Формула Haversine для расстояния между координатами:
```csharp
double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371; // радиус Земли в км
    var dLat = ToRad(lat2 - lat1);
    var dLon = ToRad(lon2 - lon1);
    var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
            Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
            Math.Sin(dLon/2) * Math.Sin(dLon/2);
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
    return R * c;
}
```

---

## Рекомендательная система (ML.NET)

### Архитектура
- **Модель**: Matrix Factorization
- **Данные**: UserId, LocationId, Rating
- **Файлы**: `ML/RecommendationModel.cs`, `ML/RecommendationData.cs`

### Процесс обучения
```csharp
var pipeline = mlContext.Recommendation().Trainers.MatrixFactorization(
    labelColumnName: "Rating",
    matrixColumnIndexColumnName: "UserId",
    matrixRowIndexColumnName: "LocationId",
    numberOfIterations: 20,
    approximationRank: 100);
```

### Fallback
При недостатке данных для ML (< 10 записей) используется рекомендация на основе:
- Фандомов, которые понравились пользователю (оценки 4–5)
- Популярности (количество отзывов)

---

## Система авторизации

### Сессии
Используются ASP.NET Core Sessions:
```csharp
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(2);
});
```

### Проверка ролей
Статические методы в `AccountController`:
```csharp
AccountController.IsAuthenticated(HttpContext)
AccountController.IsAdmin(HttpContext)
AccountController.IsPartner(HttpContext)
AccountController.GetUserId(HttpContext)
```

### Атрибут авторизации
```csharp
[AuthorizeRole(UserRole.Admin)]  // Только для администраторов
public class AdminController : Controller { }
```

---

## Экспорт отчётов

### Excel (ClosedXML)
```csharp
using var workbook = new XLWorkbook();
var worksheet = workbook.Worksheets.Add("Report");
// Заполнение данных...
worksheet.Columns().AdjustToContents();
return workbook.SaveAs(stream);
```

### Word (DocumentFormat.OpenXml)
```csharp
using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
var body = new Body();
body.Append(new Paragraph(new Run(new Text(title))));
// ...
```

---

## Запуск проекта

### Предварительные требования
- .NET 8.0 SDK

### Команды
```bash
# Сборка
dotnet build

# Запуск
cd GeekTour.Web
dotnet run

# Тесты
dotnet test
```

При первом запуске автоматически создаётся база SQLite (`geektour.db`) и заполняется тестовыми данными.

### Тестовые аккаунты

| Роль | Email | Пароль |
|------|-------|--------|
| Админ | admin@geektour.ru | Admin123! |
| Админ | adm@gmail.com | 123123 |
| Партнёр | partner@geektour.ru | Partner123! |
| Турист | tourist@geektour.ru | Tourist123! |

---

## Расширение функциональности

### Добавление нового поля в локацию
1. Добавить свойство в `Location.cs`
2. Добавить поле в `LocationEditViewModel.cs`
3. Обновить методы `CreateAsync`/`UpdateAsync` в `LocationService.cs`
4. Добавить поле в `Create.cshtml` и `Edit.cshtml`

### Добавление нового отчёта
1. Добавить метод в `IReportService`/`ReportService.cs`
2. Добавить action в `ReportController.cs`
3. Создать View `.cshtml`
4. Добавить кнопку на `Report/Index.cshtml`

### Добавление новой роли
1. Добавить значение в `UserRole` enum
2. Обновить проверки в контроллерах

---

## Известные ограничения

1. **Карты**: Используется Leaflet + OpenStreetMap (бесплатно). Для Яндекс.Карт требуется API-ключ
2. **Авторизация**: Сессионная (не JWT). Для API-клиентов рекомендуется переход на JWT
3. **Файловое хранилище**: Локальное (`wwwroot/uploads`). Для продакшена — S3/облако
4. **SQLite**: Однопользовательская БД. Для многопользовательского режима рекомендуется PostgreSQL

---

## Лицензия

Курсовой проект. Все права принадлежат разработчику.
