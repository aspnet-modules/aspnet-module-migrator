namespace AspNet.Module.Migrator;

/// <summary>
///     Мигратор
/// </summary>
public interface IAspNetMigratorHost
{
    /// <summary>
    ///     Выполнить миграцию
    /// </summary>
    Task Execute(CancellationToken ct);
}