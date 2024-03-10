using System.Reflection;
using AspNet.Module.Migrator.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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
        NpgsqlDbContextConfigurer.Configure(builder, connection);

        return CreateDbContext(builder.Options);
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
}