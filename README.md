# Foxflix

Foxflix — навчальний вебзастосунок для каталогу фільмів і серіалів на ASP.NET Core MVC + Web API, PostgreSQL, Entity Framework Core та ASP.NET Core Identity.

## Проєкти

- `Foxflix.Web` — MVC-інтерфейс.
- `Foxflix.API` — REST API, Swagger, авторизація та інтеграція TMDB.
- `Foxflix.Infrastructure` — контексти EF Core, Identity та сховище медіафайлів.
- `Netflix.Domain` — сутності предметної області.
- `Netflix.Application` — DTO та прикладний шар.
- `Netflix.Tests` — unit-тести.

## Важливе про базу даних

При старті API виконуються ідемпотентні PostgreSQL-скрипти, вбудовані в збірку. Вони створюють відсутні таблиці `Movies`, `Series`, `Seasons`, `Episodes`, `Favorites`, `WatchHistories`, `TmdbFavorites` та таблиці ASP.NET Identity. Вони не видаляють наявні таблиці або рядки.

Старі SQL Server-скрипти були прибрані, оскільки `nvarchar`, `datetime2` і `bit` не є коректною схемою для PostgreSQL.

## Локальний запуск через Visual Studio

Потрібен .NET SDK 10 та доступна PostgreSQL-база. API використовує `ConnectionStrings:DefaultConnection`, а Web читає API-адресу з `ApiSettings:BaseUrl`.

1. Відкрий `Foxflix.slnx`.
2. Встанови пароль/рядок підключення до своєї PostgreSQL-бази через User Secrets (не зберігай пароль у Git):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=YOUR_HOST;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_NEW_PASSWORD;SSL Mode=Require;Trust Server Certificate=true" --project .\Foxflix.API\Foxflix.API.csproj
```

3. Додай TMDB Read Access Token, щоб увімкнути динамічний каталог:

```powershell
dotnet user-secrets set "TMDB:ReadAccessToken" "YOUR_TMDB_READ_ACCESS_TOKEN" --project .\Foxflix.API\Foxflix.API.csproj
```

4. За потреби налаштуй адміністратора через User Secrets:

```powershell
dotnet user-secrets set "InitialAdmin:Email" "you@example.com" --project .\Foxflix.API\Foxflix.API.csproj
dotnet user-secrets set "InitialAdmin:Password" "YOUR_STRONG_PASSWORD" --project .\Foxflix.API\Foxflix.API.csproj
```

Якщо значення адміністратора не задані, API створить ролі `User`, `Moderator`, `Admin`, але не створюватиме адміністратора з відомим паролем за замовчуванням.

5. Запусти одночасно `Foxflix.API` і `Foxflix.Web` (Multiple startup projects). Локальні адреси зі launch settings: API `https://localhost:7283/swagger`, Web `https://localhost:7059`.

## Локальний запуск через Docker Compose

1. Скопіюй `.env.example` у `.env`.
2. За потреби заповни `TMDB_READ_ACCESS_TOKEN` та параметри початкового адміністратора.
3. У корені рішення виконай:

```powershell
docker compose up --build
```

Web: `http://localhost:8080`; API/Swagger: `http://localhost:8081/swagger`. Compose запускає локальний PostgreSQL з volume для даних і окремий volume для медіафайлів. Не використовуй пароль із прикладу для публічного розгортання.

## Тестування

```powershell
dotnet test .\Netflix.Tests\Foxflix.Tests.csproj
```

## Застереження щодо вмісту

TMDB потрібен для каталогу фільмів/серіалів, пошуку та постерів із зовнішнього сервісу. Сам TMDB не надає повні відеофайли для стримінгу. Локальне відтворення працює для відео, завантажених у сховище Foxflix та прив'язаних до записів у базі.

## Перевірка збірки

Під час підготовки архіву в середовищі не було встановлено .NET SDK і не було доступу до NuGet, тому `dotnet build` та `dotnet test` тут фактично не запускалися. Перед здачею відкрий solution у Visual Studio або виконай `dotnet build .\Foxflix.slnx` і `dotnet test .\Netflix.Tests\Foxflix.Tests.csproj` на комп’ютері з .NET SDK 10.
