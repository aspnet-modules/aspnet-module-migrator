using AspNet.Module.Common;
using AspNet.Module.Logging.Extensions;
using AspNet.Module.Logging.Options;
using AspNet.Module.Migrator.Database;
using AspNet.Module.Migrator.Models;
using AspNet.Module.Migrator.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace AspNet.Module.Migrator;

/// <summary>
///     Дев выполнение миграции
/// </summary>
public abstract class BaseMigratorHost<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Конфигурация логгера
    /// </summary>
    protected LoggerConfiguration ConfigureLogger(LoggerConfiguration loggerConfiguration,
        IConfiguration configuration, IHostEnvironment env)
    {
        var serilogSections = configuration.GetSection(SerilogOptions.Key);
        var serilogOptions = serilogSections.Exists()
            ? serilogSections.Get<SerilogOptions>(o => { o.BindNonPublicProperties = true; })!
            : SerilogOptions.Default;

        return loggerConfiguration.Configure(configuration, env, serilogOptions);
    }

    /// <summary>
    ///     Конфигурация сервисов
    /// </summary>
    protected void ConfigureServices(IServiceCollection services, IConfiguration configuration,
        IHostEnvironment env, Action<AspNetMigratorContext<TDbContext>>? configureContext)
    {
        var ctx = new AspNetMigratorContext<TDbContext>();
        configureContext?.Invoke(ctx);

        services.AddSingleton(configuration);
        services.AddSingleton(env);
        services.AddSingleton<ICreateDatabaseFactory<TDbContext>, DefaultCreateDatabaseFactory<TDbContext>>();
        services.AddScoped<MigrateDatabaseProvider<TDbContext>>();
        services.AddScoped<SeedDatabaseProvider<TDbContext>>();
        services.AddSingleton<ISeedDatabaseHelper>(new DefaultSeedDatabaseHelper(ctx.EntryAssembly));

        var seedType = typeof(ISeedDatabaseExecutor<TDbContext>);
        var seedList = new SeedList<TDbContext>();
        ctx.Seeds?.Invoke(seedList);

        foreach (var seed in seedList)
        {
            services.AddScoped(seedType, seed);
        }

        var ctModuleContext = new AspNetModuleContext
        {
            Configuration = configuration,
            Environment = env,
            Services = services,
            EntryAssembly = ctx.EntryAssembly
        };

        var moduleList = new ModuleList();
        ctx.Modules?.Invoke(moduleList);

        foreach (var module in moduleList)
        {
            module.Configure(ctModuleContext);
        }

        ctx.Services?.Invoke(services);
    }

    /// <summary>
    ///     Миграция и сиды
    /// </summary>
    protected async Task MigrateAndSeed(IServiceScope scope, string? connStr,
        IMigratorInternalConfig<TDbContext> internalConfig,
        bool seed, CancellationToken ct)
    {
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<TDbContext>>();
        var databaseFactory = sp.GetRequiredService<ICreateDatabaseFactory<TDbContext>>();
        var migrateDatabaseProvider = sp.GetRequiredService<MigrateDatabaseProvider<TDbContext>>();

        logger.LogWarning("------> Запуск мигратора. Не останавливать! <------");

        var env = sp.GetRequiredService<IHostEnvironment>();
        logger.LogDebug($"<------> Мигратор - {env.ApplicationName} [{env.EnvironmentName}] <------>");

        await using var migrateDbContext = databaseFactory.Create(connStr, internalConfig);
        await migrateDatabaseProvider.Execute(migrateDbContext, ct);

        if (seed)
        {
            var seedDatabaseProvider = sp.GetRequiredService<SeedDatabaseProvider<TDbContext>>();
            await using var seedDbContext = databaseFactory.Create(connStr, internalConfig);
            await seedDatabaseProvider.Execute(seedDbContext, internalConfig, ct);
        }

        logger.LogWarning("------> Мигратор выполнил все задачи! <------");
    }
}