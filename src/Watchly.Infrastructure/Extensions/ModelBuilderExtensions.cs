using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Watchly.Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseSnakeCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            entityType.SetTableName(ToSnakeCase(tableName));

            foreach (var property in entityType.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name));

            foreach (var key in entityType.GetKeys())
            {
                var name = key.GetName();
                if (!string.IsNullOrEmpty(name))
                    key.SetName(ToSnakeCase(name));
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                var name = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(name))
                    fk.SetConstraintName(ToSnakeCase(name));
            }

            foreach (var index in entityType.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(name))
                    index.SetDatabaseName(ToSnakeCase(name));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder();
        var previousCategory = default(char?);

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && previousCategory != '_' && (char.IsLower(previousCategory ?? c) || (i + 1 < input.Length && char.IsLower(input[i + 1]))))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }

            previousCategory = c;
        }

        return sb.ToString();
    }
}