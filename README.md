# AspNet.Module.Migrator

Database migration package for applying EF Core migrations and running seed data.

## Usage Steps

### 1. Create a migration DbContext

```cs
public class MigrationDbContext : AppDbContext
{
}
```

### 2. Add `Microsoft.EntityFrameworkCore.Design`

This enables `dotnet ef migrations add` and other EF Core CLI commands.

```xml
<ItemGroup>
    <PackageReference Include="AspNet.Module.Migrator" Version="3.x.x" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

### 3. Create a design-time DbContext factory

```cs
public class MigrationDbContextFactory : BaseDesignTimeDbContextFactory<MigrationDbContext>
{
}
```

### 4. Generate migrations from CLI

```sh
dotnet ef migrations add Init
```

```sh
dotnet ef migrations remove
```

### 5. Configure the migrator in `Program`

```cs
var migrator = new AspNetMigrator<MigrationDbContext>(new AspNetMigratorOptions(args)
{
    Configuration = c =>
    {
#if DEBUG
        c.AddUserSecrets(Assembly.GetEntryAssembly());
#endif
    }
});

migrator.Seeds.Add<Seed1>();
migrator.Seeds.Add<Seed2>();

await migrator.RunAsync();
```

### 6. Add the migrator to a Docker image

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
RUN dotnet restore src/Some.Migrator/*.csproj
WORKDIR /app/src/Some.Migrator
RUN dotnet publish -o out -c Release --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
COPY --from=build /app/src/Some.Migrator/out ./migrate/
```

## Seed Registration

```cs
internal class SomeSeed : ISeedDatabaseExecutor<MigrationDbContext>
{
    public int Order => 1;
    public string Name => nameof(SomeSeed);
    public bool Enabled => true;

    public Task Execute(MigrationDbContext dbContext, CancellationToken ct)
    {
        dbContext.SomeEntity.Add(...);
        return Task.CompletedTask;
    }
}
```

```cs
var migrator = new AspNetMigrator<MigrationDbContext>(new ConsoleMigratorConfig(...));
migrator.Seeds.Add<SomeSeed>();
await migrator.RunAsync();
```

## Source Code

- Repository: [github.com/aspnet-modules/aspnet-module-migrator](https://github.com/aspnet-modules/aspnet-module-migrator)
