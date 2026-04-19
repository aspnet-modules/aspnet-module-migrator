# Модуль миграции БД

Модуль позволяет применять миграции и последющий Seed (пополнение БД данными)

## Шаги использования

### 1. Создать БД контекст от исходного

Шаг позволит создать папку с миграциями в проекте Migrator

```cs
public class MigrationDbContext : AppDbContext
{

}
```

### 2. Добавить пакет Microsoft.EntityFrameworkCore.Design

Добавляет возможность вызывать cli методы для создания миграций `dotnet ef migrations add`.

> В файле csproj появится блок

```xml
<ItemGroup>
    <PackageReference Include="AspNet.Module.Migrator" Version="1.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
``` 

### 3. Создать MigrationDbContextFactory с интерфейсом IDesignTimeDbContextFactory<MigrationDbContext>

Также необходим для вызова cli методов `dotnet ef ...`. Фабрика позволяет создать БД контекст с указанной строкой
подключения.

> MigrationDbContextFactory.cs

```cs
// BaseDesignTimeDbContextFactory - реализует интерфейс IDesignTimeDbContextFactory
public class MigrationDbContextFactory : BaseDesignTimeDbContextFactory<MigrationDbContext>
{

}
```

### 4. Вызвать методы cli для генерации миграций

Переходи в папку проекта в термиале и вызываем `dotnet ef migrations add Название миграции`.
При этом нужно заранее установить [dotnet cli](https://docs.microsoft.com/ru-ru/ef/core/cli/dotnet)

```sh
# Создание миграции с названием Init
dotnet ef migrations add Init
```

```sh
# Удаление последней миграции
dotnet ef migrations remove
```

### 5. Модифицировать Program

Инициализация и конфигурация мигратора и последующий запуск

> Program.cs

```cs
var migrator = new AspNetMigrator<MigrationDbContext>(new AspNetMigratorOptions(args)
{
    Configuration = c =>
    {
    // используем User Secrets только в сборке Debug
#if DEBUG
        c.AddUserSecrets(Assembly.GetEntryAssembly());
#endif
    }
});

// добавляем сиды
migrator.Seeds.Add<Seed1>();
migrator.Seeds.Add<Seed2>();

await migrator.RunAsync();
```

### 6. Конфигурация Docker образа для добавления мигратора

Обычно Docker образ состоит из двух частей:

- FROM Build - собирает приложение
- FROM Run - запускает приложение

> Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Host --->
# build host project
# Host <---

# Migrator ---> 
WORKDIR /app
RUN dotnet restore src/Some.Migrator/*.csproj
WORKDIR /app/src/Some.Migrator
RUN dotnet publish -o out -c Release --no-restore
WORKDIR /app
# Migrator <---

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# Host --->
# COPY Some.Host/out

# Migrator ---> 
COPY --from=build /app/src/Some.Migrator/out ./migrate/

```

## Добавление сидов

Добавление данных после применения миграции.

### 1. Создать сид с интерфейсом ISeedDatabaseExecutor<MigrationDbContext>

> SomeSeed.cs

```cs
internal class SomeSeed : ISeedDatabaseExecutor<MigrationDbContext>
{
    // сортировка сидов по возрастанию
    public int Order => 1;

    // название сида
    public string Name => nameof(SomeSeed);

    // можно запустить сид в зависимости от окружения
    public bool Enabled => true;
    
    public Task Execute(MigrationDbContext dbContext, CancellationToken ct)
    {
        // манипуляции с данными. Не вызываем SaveChanges, тк менеджер сидов запускает после применения всех сидов
        dbContext.SomeEntity.Add(...)
        return Task.CompletedTask;
    }
}
```

### 2. Зарегистрировать сид

Регистрация сида в миграторе.

```cs
var migrator = new AspNetMigrator<MigrationDbContext>(new ConsoleMigratorConfig(...));
var migrator = new AspNetMigrator<MigrationDbContext>(new WebMigratorConfig(...));

// добавление сида
migrator.Seeds.Add<SomeSeed>();

await migrator.RunAsync();
```
