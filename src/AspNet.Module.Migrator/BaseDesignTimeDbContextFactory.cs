using System.Reflection;
using AspNet.Module.Dal.EfCore.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using DbConnectionStr = AspNet.Module.Migrator.Database.DbConnectionStr;

namespace AspNet.Module.Migrator;

/// <summary>
///     Базовый класс для создания <see cref="IDesignTimeDbContextFactory{TContext}" />
/// </summary>
public abstract class BaseDesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    /// <inheritdoc />
    public TDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<TDbContext>();
        var connection = GetConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connection);
        var dataSource = dataSourceBuilder.Build();
        var migrationsHistorySchema = GetMigrationsHistorySchema();
        NpgsqlDbContextConfigurer.Configure(builder, dataSource, ConfigureNpgsql, migrationsHistorySchema);
        ConfigureBuilder(builder);

        return CreateDbContext(builder.Options);
    }

    /// <summary>
    ///     Настройка DbContextOptionsBuilder
    /// </summary>
    protected virtual void ConfigureBuilder(DbContextOptionsBuilder<TDbContext> builder)
    {
    }

    /// <summary>
    ///     Настройка Npgsql
    /// </summary>
    protected virtual void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder builder)
    {
    }

    /// <summary>
    ///     Создание БД контекста
    /// </summary>
    protected virtual TDbContext CreateDbContext(DbContextOptions<TDbContext> options)
    {
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), flags, null,
            new object[] { options }, null)!;
    }

    /// <summary>
    ///     Получить строку подключения к БД
    /// </summary>
    protected virtual string GetConnectionString()
    {
        // By user secrets
        var userSecretsAssembly = GetType().Assembly;
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets(userSecretsAssembly, true);
        var connStr = DbConnectionStr.FromConfiguration(configurationBuilder.Build());
        ArgumentNullException.ThrowIfNull(connStr, nameof(connStr));
        return connStr;
    }

    /// <summary>
    ///     Получить схему миграции
    /// </summary>
    /// <returns></returns>
    protected virtual string GetMigrationsHistorySchema() => "public";
}