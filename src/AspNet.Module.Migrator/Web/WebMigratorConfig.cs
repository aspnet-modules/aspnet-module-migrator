using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNet.Module.Migrator.Web;

/// <summary>
///     Конфигурация Web
/// </summary>
public class WebMigratorConfig<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     ctor
    /// </summary>
    public WebMigratorConfig(IConfiguration configuration, IHostEnvironment env)
    {
        Configuration = configuration;
        Env = env;
    }

    /// <summary>
    ///     Конфигурация
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    ///     Кастомный коннект к бд
    /// </summary>
    public string? ConnStr { get; init; }

    /// <summary>
    ///     Настрока контекста
    /// </summary>
    public Action<AspNetMigratorContext<TDbContext>>? Context { get; init; }

    /// <summary>
    ///     <see cref="IHostEnvironment" />
    /// </summary>
    public IHostEnvironment Env { get; }

    /// <summary>
    ///     Конфигурация <see cref="NpgsqlDbContextOptionsBuilder" />
    /// </summary>
    public Action<NpgsqlDbContextOptionsBuilder>? NpgsqlConfigure { get; init; }

    /// <summary>
    ///     Запустить сиды
    /// </summary>
    public bool Seed { get; init; }
}