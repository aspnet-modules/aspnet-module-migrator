using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;

namespace AspNet.Module.Migrator.Extensions;

/// <summary>
///     Расширения для <see cref="DatabaseFacade" />
/// </summary>
internal static class DatabaseExtensions
{
    /// <summary>
    ///     Перезагрузка <see cref="Enum" /> типов в БД после миграции
    /// </summary>
    /// <param name="facade">Фасад БД</param>
    public static void ReloadEnumTypes(this DatabaseFacade facade)
    {
        // https://github.com/npgsql/efcore.pg/issues/513#issuecomment-405290695
        var connection = (NpgsqlConnection)facade.GetDbConnection();
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        connection.ReloadTypes();
    }
}