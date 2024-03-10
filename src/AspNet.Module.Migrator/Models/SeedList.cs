using System.Collections;
using AspNet.Module.Migrator.Seeds;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator.Models;

/// <summary>
///     Билдер сидов
/// </summary>
public class SeedList<TDbContext> : IEnumerable<Type>
    where TDbContext : DbContext
{
    private readonly HashSet<Type> _seedTypes;

    internal SeedList()
    {
        _seedTypes = new HashSet<Type>();
    }

    /// <summary>
    ///     Добавить сид
    /// </summary>
    /// <typeparam name="TSeed"></typeparam>
    public void Add<TSeed>() where TSeed : ISeedDatabaseExecutor<TDbContext> => _seedTypes.Add(typeof(TSeed));

    /// <inheritdoc />
    public IEnumerator<Type> GetEnumerator() => _seedTypes.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}