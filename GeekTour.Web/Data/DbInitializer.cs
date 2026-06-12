using GeekTour.Web.Models.Entities;
using GeekTour.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GeekTour.Web.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // Add AvatarPath column if it doesn't exist (migration for existing DBs)
        try
        {
            context.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN AvatarPath TEXT");
        }
        catch { /* ignore if column already exists */ }

        if (context.Users.Any()) return; // Already seeded

        // ===================== CATEGORIES =====================
        var categories = new Category[]
        {
            new() { Name = "Кафе" },
            new() { Name = "Магазин" },
            new() { Name = "Арт-объект" },
            new() { Name = "Книжный" },
            new() { Name = "Антикафе" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        // ===================== FANDOMS =====================
        var fandoms = new Fandom[]
        {
            new() { Name = "Аниме",        Category = FandomCategory.Anime },
            new() { Name = "Манга",        Category = FandomCategory.Anime },
            new() { Name = "Киберпанк",    Category = FandomCategory.VideoGames },
            new() { Name = "Фэнтези",      Category = FandomCategory.BoardGames },
            new() { Name = "Dungeons & Dragons", Category = FandomCategory.BoardGames },
            new() { Name = "Marvel",       Category = FandomCategory.Comics },
            new() { Name = "DC Comics",    Category = FandomCategory.Comics },
            new() { Name = "Видеоигры",   Category = FandomCategory.VideoGames },
            new() { Name = "Настолки",     Category = FandomCategory.BoardGames },
            new() { Name = "Киберпанк 2077", Category = FandomCategory.VideoGames }
        };
        context.Fandoms.AddRange(fandoms);
        context.SaveChanges();

        // ===================== USERS =====================
        var users = new User[]
        {
            new() { Email = "admin@geektour.ru",  PasswordHash = HashPass("Admin123!"), Name = "Администратор", Role = UserRole.Admin },
            new() { Email = "adm@gmail.com",      PasswordHash = HashPass("123123"),    Name = "Админ",        Role = UserRole.Admin },
            new() { Email = "partner@geektour.ru", PasswordHash = HashPass("Partner123!"), Name = "Партнёр Иванов", Role = UserRole.Partner, CompanyName = "AnimeCafe LLC", INN = "7712345678", IsVerified = true },
            new() { Email = "tourist@geektour.ru", PasswordHash = HashPass("Tourist123!"), Name = "Турист Петров",  Role = UserRole.Tourist },
            new() { Email = "partner2@geektour.ru", PasswordHash = HashPass("Partner123!"), Name = "Партнёр Сидоров", Role = UserRole.Partner, CompanyName = "ComicShop", INN = "7798765432", IsVerified = true }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // ===================== LOCATIONS =====================
        // partner@geektour.ru = users[2] (IsVerified Partner)
        var partnerId = users[2].Id;

        var locations = new Location[]
        {
            new()
            {
                Name = "Anime Café Tokyo",
                Description = "Тематическое кафе в стиле аниме. Меню с блюдами из аниме-сериалов, уютный интерьер с фигурками и постерами.",
                Type = LocationType.Cafe,
                Address = "пр-т Победителей, 17, Минск",
                Latitude = 53.9085,
                Longitude = 27.5470,
                WorkingHoursJson = "{\"monday\":\"10:00-23:00\",\"tuesday\":\"10:00-23:00\",\"wednesday\":\"10:00-23:00\",\"thursday\":\"10:00-23:00\",\"friday\":\"10:00-01:00\",\"saturday\":\"10:00-01:00\",\"sunday\":\"11:00-22:00\"}",
                CategoryId = 1, // Кафе
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "Comic World",
                Description = "Магазин комиксов, манги и фигурок. Широкий выбор Marvel, DC, японская манга.",
                Type = LocationType.Shop,
                Address = "ул. Коммунистическая, 33, Минск",
                Latitude = 53.8965,
                Longitude = 27.5515,
                WorkingHoursJson = "{\"monday\":\"11:00-21:00\",\"tuesday\":\"11:00-21:00\",\"wednesday\":\"11:00-21:00\",\"thursday\":\"11:00-21:00\",\"friday\":\"11:00-22:00\",\"saturday\":\"10:00-22:00\",\"sunday\":\"11:00-20:00\"}",
                CategoryId = 2, // Магазин
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "CyberBar Neo",
                Description = "Бар в стиле киберпанк. Неоновое освещение, коктейли с футуристическими названиями, видеоигровые автоматы.",
                Type = LocationType.Cafe,
                Address = "ул. Кирова, 11, Минск",
                Latitude = 53.8980,
                Longitude = 27.5730,
                WorkingHoursJson = "{\"monday\":\"14:00-02:00\",\"tuesday\":\"14:00-02:00\",\"wednesday\":\"14:00-02:00\",\"thursday\":\"14:00-03:00\",\"friday\":\"14:00-05:00\",\"saturday\":\"12:00-05:00\",\"sunday\":\"14:00-00:00\"}",
                CategoryId = 1, // Кафе
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "D&D Tavern",
                Description = "Антикафе для настолочников. Огромная библиотека настольных игр, мастера для D&D, Magic: The Gathering турниры.",
                Type = LocationType.Cafe,
                Address = "ул. Козлова, 28, Минск",
                Latitude = 53.9020,
                Longitude = 27.5650,
                WorkingHoursJson = "{\"monday\":\"12:00-23:00\",\"tuesday\":\"12:00-23:00\",\"wednesday\":\"12:00-23:00\",\"thursday\":\"12:00-00:00\",\"friday\":\"12:00-02:00\",\"saturday\":\"10:00-02:00\",\"sunday\":\"10:00-22:00\"}",
                CategoryId = 5, // Антикафе
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "Pixel Art Gallery",
                Description = "Галерея пиксель-арта и ретро-игровой культуры. Выставки инди-художников, ретро-консоли для игры.",
                Type = LocationType.ArtObject,
                Address = "ул. Горецкого, 2, Минск",
                Latitude = 53.9105,
                Longitude = 27.5380,
                WorkingHoursJson = "{\"monday\":\"closed\",\"tuesday\":\"12:00-20:00\",\"wednesday\":\"12:00-20:00\",\"thursday\":\"12:00-20:00\",\"friday\":\"12:00-21:00\",\"saturday\":\"11:00-21:00\",\"sunday\":\"11:00-19:00\"}",
                CategoryId = 3, // Арт-объект
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "GameStation Shop",
                Description = "Магазин видеоигр, консолей и аксессуаров. Ретро и новые релизы, обмен и продажа б/у игр.",
                Type = LocationType.Shop,
                Address = "ул. Свердлова, 8, Минск",
                Latitude = 53.8955,
                Longitude = 27.5600,
                WorkingHoursJson = "{\"monday\":\"10:00-21:00\",\"tuesday\":\"10:00-21:00\",\"wednesday\":\"10:00-21:00\",\"thursday\":\"10:00-21:00\",\"friday\":\"10:00-22:00\",\"saturday\":\"10:00-22:00\",\"sunday\":\"11:00-20:00\"}",
                CategoryId = 2, // Магазин
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "Мир Фигурок",
                Description = "Специализированный магазин коллекционных фигурок: Funko Pop, Nendoroid, модели из аниме и комиксов.",
                Type = LocationType.Shop,
                Address = "ул. Ленина, 44, Минск",
                Latitude = 53.8990,
                Longitude = 27.5445,
                WorkingHoursJson = "{\"monday\":\"11:00-20:00\",\"tuesday\":\"11:00-20:00\",\"wednesday\":\"11:00-20:00\",\"thursday\":\"11:00-20:00\",\"friday\":\"11:00-21:00\",\"saturday\":\"10:00-21:00\",\"sunday\":\"12:00-18:00\"}",
                CategoryId = 2, // Магазин
                OwnerId = partnerId,
                IsActive = true
            },
            new()
            {
                Name = "Гик-Стена",
                Description = "Стена-инсталляция с муралами в стиле гик-культуры. Граффити с персонажами из видеоигр, аниме и комиксов. Фотозона.",
                Type = LocationType.ArtObject,
                Address = "ул. Мельникайте, 12, Минск",
                Latitude = 53.9050,
                Longitude = 27.5520,
                WorkingHoursJson = "{\"monday\":\"00:00-23:59\",\"tuesday\":\"00:00-23:59\",\"wednesday\":\"00:00-23:59\",\"thursday\":\"00:00-23:59\",\"friday\":\"00:00-23:59\",\"saturday\":\"00:00-23:59\",\"sunday\":\"00:00-23:59\"}",
                CategoryId = 3, // Арт-объект
                OwnerId = partnerId,
                IsActive = true
            }
        };
        context.Locations.AddRange(locations);
        context.SaveChanges();

        context.LocationFandoms.AddRange(new LocationFandom[]
        {
            new() { LocationId = locations[0].Id, FandomId = fandoms[0].Id },
            new() { LocationId = locations[0].Id, FandomId = fandoms[1].Id },
            new() { LocationId = locations[1].Id, FandomId = fandoms[1].Id },
            new() { LocationId = locations[1].Id, FandomId = fandoms[5].Id },
            new() { LocationId = locations[1].Id, FandomId = fandoms[6].Id },
            new() { LocationId = locations[2].Id, FandomId = fandoms[2].Id },
            new() { LocationId = locations[2].Id, FandomId = fandoms[7].Id },
            new() { LocationId = locations[2].Id, FandomId = fandoms[9].Id },
            new() { LocationId = locations[3].Id, FandomId = fandoms[3].Id },
            new() { LocationId = locations[3].Id, FandomId = fandoms[4].Id },
            new() { LocationId = locations[3].Id, FandomId = fandoms[8].Id },
            new() { LocationId = locations[4].Id, FandomId = fandoms[7].Id },
            new() { LocationId = locations[4].Id, FandomId = fandoms[2].Id },
            // GameStation Shop — VideoGames, Cyberpunk
            new() { LocationId = locations[5].Id, FandomId = fandoms[7].Id },
            new() { LocationId = locations[5].Id, FandomId = fandoms[2].Id },
            new() { LocationId = locations[5].Id, FandomId = fandoms[9].Id },
            // Мир Фигурок — Anime, Comics, BoardGames
            new() { LocationId = locations[6].Id, FandomId = fandoms[0].Id },
            new() { LocationId = locations[6].Id, FandomId = fandoms[5].Id },
            new() { LocationId = locations[6].Id, FandomId = fandoms[8].Id },
            // Гик-Стена — VideoGames, Anime, Comics
            new() { LocationId = locations[7].Id, FandomId = fandoms[7].Id },
            new() { LocationId = locations[7].Id, FandomId = fandoms[0].Id },
            new() { LocationId = locations[7].Id, FandomId = fandoms[5].Id },
        });
        context.SaveChanges();

        var touristId = users[3].Id;
        var reviews = new Review[]
        {
            new() { UserId = touristId, LocationId = locations[0].Id, Rating = 5, Text = "Потрясающее кафе! Атмосфера как в Токио, очень вкусные рамены и десерты в стиле аниме.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new() { UserId = touristId, LocationId = locations[1].Id, Rating = 4, Text = "Отличный выбор манги и комиксов. Цены немного кусаются, но ассортимент радует.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { UserId = touristId, LocationId = locations[2].Id, Rating = 5, Text = "Лучший бар в стиле киберпанк! Неон, коктейли, атмосфера — всё на высоте.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new() { UserId = touristId, LocationId = locations[3].Id, Rating = 4, Text = "Отличное место для настолочников. Большой выбор игр, приятная атмосфера.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { UserId = touristId, LocationId = locations[4].Id, Rating = 5, Text = "Интересная галерея! Пиксель-арт впечатляет, особенно ретро-секция.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { UserId = touristId, LocationId = locations[0].Id, Rating = 4, Text = "Второй визит — тоже круто! Новые позиции в меню.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { UserId = touristId, LocationId = locations[1].Id, Rating = 3, Text = "Хороший магазин, но цены могли бы быть ниже.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { UserId = touristId, LocationId = locations[2].Id, Rating = 4, Text = "Рекомендую! Коктейли вкусные, атмосфера атмосферная.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = touristId, LocationId = locations[3].Id, Rating = 5, Text = "Провели тут целый день! D&D сессия была огонь.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { UserId = touristId, LocationId = locations[4].Id, Rating = 3, Text = "Неплохо, но хотелось бы больше интерактива.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            // New locations reviews
            new() { UserId = touristId, LocationId = locations[5].Id, Rating = 5, Text = "Огромный выбор игр! Нашёл редкую копию для PS2. Продавцы знают своё дело.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { UserId = touristId, LocationId = locations[5].Id, Rating = 4, Text = "Хороший магазин, но хотелось бы больше ретро-консолей в наличии.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-6) },
            new() { UserId = touristId, LocationId = locations[6].Id, Rating = 5, Text = "Наконец-то магазин где есть Nendoroid из новых аниме! Цены адекватные.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { UserId = touristId, LocationId = locations[6].Id, Rating = 4, Text = "Отличная коллекция фигурок. Есть редкие экземпляры.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new() { UserId = touristId, LocationId = locations[7].Id, Rating = 5, Text = "Невероятная стена! Фоткались всем клубом. Обязательно к посещению.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { UserId = touristId, LocationId = locations[7].Id, Rating = 4, Text = "Красиво и атмосферно. Хотелось бы, чтобы обновляли муралы чаще.", IsModerated = true, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            // Reviews on moderation
            new() { UserId = touristId, LocationId = locations[0].Id, Rating = 2, Text = "Что-то не очень, обслуживание медленное.", IsModerated = false, CreatedAt = DateTime.UtcNow }
        };
        context.Reviews.AddRange(reviews);
        context.SaveChanges();

        var promotions = new Promotion[]
        {
            new() { LocationId = locations[0].Id, Title = "Скидка 20% на рамен", Description = "При заказе любого рамена — скидка 20% весь день", StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(30) },
            new() { LocationId = locations[1].Id, Title = "Купи 2 манги — получи 3-ю в подарок", Description = "Акция на все тома манги издательства Истари", StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(14) },
            new() { LocationId = locations[2].Id, Title = "Счастливые часы", Description = "С 14:00 до 18:00 все коктейли по половинной цене", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(60) }
        };
        context.Promotions.AddRange(promotions);
        context.SaveChanges();

        // ===================== EVENTS =====================
        var events = new EventItem[]
        {
            new() { LocationId = locations[3].Id, Title = "D&D Ночь", Description = "Открытые сессии Dungeons & Dragons для всех уровней. Мастер предоставляется.", EventDate = DateTime.UtcNow.AddDays(7), IsActive = true },
            new() { LocationId = locations[1].Id, Title = "Премьера нового тома манги", Description = "Презентация и автограф-сессия автора", EventDate = DateTime.UtcNow.AddDays(14), IsActive = true },
            new() { LocationId = locations[4].Id, Title = "Ретро-игровой вечер", Description = "Турниры по старым консолям: Dendy, Sega, SNES", EventDate = DateTime.UtcNow.AddDays(10), IsActive = true }
        };
        context.Events.AddRange(events);
        context.SaveChanges();

        // ===================== ROUTES & ROUTE POINTS =====================
        var routes = new TourRoute[]
        {
            new() { UserId = touristId, Name = "Аниме-тур по Минску", Description = "Маршрут для любителей аниме и манги", CreatedAt = DateTime.UtcNow.AddDays(-20), IsPublic = true },
            new() { UserId = touristId, Name = "Вечер киберпанка", Description = "Бары и арт-объекты в стиле киберпанк", CreatedAt = DateTime.UtcNow.AddDays(-15), IsPublic = true },
            new() { UserId = touristId, Name = "Настолочный день", Description = "Лучшие места для настольных игр", CreatedAt = DateTime.UtcNow.AddDays(-10), IsPublic = false },
            new() { UserId = touristId, Name = "Комикс-тур", Description = "Магазины комиксов и связанные места", CreatedAt = DateTime.UtcNow.AddDays(-5), IsPublic = true }
        };
        context.Routes.AddRange(routes);
        context.SaveChanges();

        var routePoints = new RoutePoint[]
        {
            // Route 1: Аниме-тур
            new() { RouteId = routes[0].Id, LocationId = locations[0].Id, OrderIndex = 0, PlannedVisitTime = DateTime.UtcNow.AddDays(-20).AddHours(11), TravelTimeFromPreviousMinutes = 0 },
            new() { RouteId = routes[0].Id, LocationId = locations[1].Id, OrderIndex = 1, PlannedVisitTime = DateTime.UtcNow.AddDays(-20).AddHours(14), TravelTimeFromPreviousMinutes = 20 },
            new() { RouteId = routes[0].Id, LocationId = locations[3].Id, OrderIndex = 2, PlannedVisitTime = DateTime.UtcNow.AddDays(-20).AddHours(17), TravelTimeFromPreviousMinutes = 30 },
            // Route 2: Вечер киберпанка
            new() { RouteId = routes[1].Id, LocationId = locations[2].Id, OrderIndex = 0, PlannedVisitTime = DateTime.UtcNow.AddDays(-15).AddHours(18), TravelTimeFromPreviousMinutes = 0 },
            new() { RouteId = routes[1].Id, LocationId = locations[4].Id, OrderIndex = 1, PlannedVisitTime = DateTime.UtcNow.AddDays(-15).AddHours(21), TravelTimeFromPreviousMinutes = 25 },
            // Route 3: Настолочный день
            new() { RouteId = routes[2].Id, LocationId = locations[3].Id, OrderIndex = 0, PlannedVisitTime = DateTime.UtcNow.AddDays(-10).AddHours(10), TravelTimeFromPreviousMinutes = 0 },
            new() { RouteId = routes[2].Id, LocationId = locations[0].Id, OrderIndex = 1, PlannedVisitTime = DateTime.UtcNow.AddDays(-10).AddHours(15), TravelTimeFromPreviousMinutes = 15 },
            // Route 4: Комикс-тур
            new() { RouteId = routes[3].Id, LocationId = locations[1].Id, OrderIndex = 0, PlannedVisitTime = DateTime.UtcNow.AddDays(-5).AddHours(12), TravelTimeFromPreviousMinutes = 0 },
            new() { RouteId = routes[3].Id, LocationId = locations[4].Id, OrderIndex = 1, PlannedVisitTime = DateTime.UtcNow.AddDays(-5).AddHours(16), TravelTimeFromPreviousMinutes = 35 },
            new() { RouteId = routes[3].Id, LocationId = locations[2].Id, OrderIndex = 2, PlannedVisitTime = DateTime.UtcNow.AddDays(-5).AddHours(20), TravelTimeFromPreviousMinutes = 40 },
        };
        context.RoutePoints.AddRange(routePoints);
        context.SaveChanges();

        // ===================== VIEW STATISTICS =====================
        var random = new Random(42);
        var viewStats = new List<ViewStatistics>();
        var searchQueries = new[] { "аниме", "комиксы", "бар", "настолки", "кафе", "киберпанк", "", "", "", "" };
        foreach (var loc in locations)
        {
            for (int i = 0; i < 30; i++)
            {
                var daysAgo = random.Next(1, 60);
                var viewDate = DateTime.UtcNow.AddDays(-daysAgo).AddHours(random.Next(9, 22));
                var hasSearch = random.Next(10) < 3;
                viewStats.Add(new ViewStatistics
                {
                    LocationId = loc.Id,
                    ViewDate = viewDate,
                    UserId = random.Next(5) == 0 ? (int?)touristId : null,
                    SearchQuery = hasSearch ? searchQueries[random.Next(searchQueries.Length)] : null
                });
            }
        }
        context.ViewStatistics.AddRange(viewStats);
        context.SaveChanges();

        // ===================== ADDITIONAL USERS (for monthly stats) =====================
        var extraUsers = new User[]
        {
            new() { Email = "user1@test.ru", PasswordHash = HashPass("123123"), Name = "Алиса", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddMonths(-5).AddDays(3) },
            new() { Email = "user2@test.ru", PasswordHash = HashPass("123123"), Name = "Борис", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddMonths(-4).AddDays(10) },
            new() { Email = "user3@test.ru", PasswordHash = HashPass("123123"), Name = "Виктор", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddMonths(-3).AddDays(7) },
            new() { Email = "user4@test.ru", PasswordHash = HashPass("123123"), Name = "Галина", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddMonths(-2).AddDays(15) },
            new() { Email = "user5@test.ru", PasswordHash = HashPass("123123"), Name = "Дмитрий", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddMonths(-1).AddDays(5) },
            new() { Email = "user6@test.ru", PasswordHash = HashPass("123123"), Name = "Елена", Role = UserRole.Tourist, RegisteredAt = DateTime.UtcNow.AddDays(-10) },
        };
        context.Users.AddRange(extraUsers);
        context.SaveChanges();
    }

    private static string HashPass(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "GeekTourSalt2024"));
        return Convert.ToBase64String(bytes);
    }
}
