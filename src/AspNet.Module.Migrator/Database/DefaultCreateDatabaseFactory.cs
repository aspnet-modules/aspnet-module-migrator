using System.Reflection;
using AspNet.Module.Dal.EfCore.Database;
using AspNet.Module.Migrator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AspNet.Module.Migrator.Database;

/// <inheritdoc />
internal class DefaultCreateDatabaseFactory<TDbContext> : ICreateDatabaseFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Создание объекта
    /// </summary>
    public DefaultCreateDatabaseFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public TDbContext Create(string? connStr, IMigratorInternalConfig<TDbContext> internalConfig)
    {
        connStr = DbConnectionStr.Normalize(connStr) ?? DbConnectionStr.FromConfiguration(_configuration);
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        internalConfig.DbContext?.Invoke(optionsBuilder);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
        var dataSource = dataSourceBuilder.Build();

        NpgsqlDbContextConfigurer.Configure(optionsBuilder, dataSource, builder =>
        {
            var migrationAssembly = typeof(TDbContext).Assembly.FullName;
            builder.MigrationsAssembly(migrationAssembly);
            internalConfig.NpgsqlContext?.Invoke(builder);
        }, internalConfig.MigratorSchema);

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), flags, null,
            new object[] { optionsBuilder.Options }, null)!;
    }
}