using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public TDbContext Create(string? connStr, Action<NpgsqlDbContextOptionsBuilder>? configure = null)
    {
        connStr = DbConnectionStr.Normalize(connStr) ?? DbConnectionStr.FromConfiguration(_configuration);
        DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        NpgsqlDbContextConfigurer.Configure(optionsBuilder, connStr, builder =>
        {
            var migrationAssembly = typeof(TDbContext).Assembly.FullName;
            builder.MigrationsAssembly(migrationAssembly);
            configure?.Invoke(builder);
        });

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), flags, null,
            new object[] { optionsBuilder.Options }, null)!;
    }
}