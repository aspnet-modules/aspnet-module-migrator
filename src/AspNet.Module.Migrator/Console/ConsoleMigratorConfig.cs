using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNet.Module.Migrator.Console;

/// <summary>
///     Опции мигратора
/// </summary>
public class ConsoleMigratorConfig<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Конструктор
    /// </summary>
    public ConsoleMigratorConfig(string[] args)
    {
        Args = args;
    }

    /// <summary>
    ///     Аргументы приложения
    /// </summary>
    public string[] Args { get; }

    /// <summary>
    ///     Конфигурация <see cref="IConfiguration" />
    /// </summary>
    public Action<IConfigurationBuilder>? Configuration { get; init; }

    /// <summary>
    ///     Конфигурация контекта
    /// </summary>
    public Action<AspNetMigratorContext<TDbContext>>? Context { get; init; }

    /// <summary>
    ///     Конфигурация <see cref="NpgsqlDbContextOptionsBuilder" />
    /// </summary>
    public Action<NpgsqlDbContextOptionsBuilder>? NpgsqlConfigure { get; init; }
}