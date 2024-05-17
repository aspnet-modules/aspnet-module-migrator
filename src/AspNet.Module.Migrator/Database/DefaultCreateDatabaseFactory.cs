using System.Reflection;
using AspNet.Module.Dal.EfCore.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

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
    public TDbContext Create(string? connStr, Action<NpgsqlDbContextOptionsBuilder>? configure, 
        string? migrationsHistorySchema)
    {
        connStr = DbConnectionStr.Normalize(connStr) ?? DbConnectionStr.FromConfiguration(_configuration);
        DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
        var dataSource = dataSourceBuilder.Build();

        NpgsqlDbContextConfigurer.Configure(optionsBuilder, dataSource, builder =>
        {
            var migrationAssembly = typeof(TDbContext).Assembly.FullName;
            builder.MigrationsAssembly(migrationAssembly);
            configure?.Invoke(builder);
        }, migrationsHistorySchema);

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), flags, null,
            new object[] { optionsBuilder.Options }, null)!;
    }
}