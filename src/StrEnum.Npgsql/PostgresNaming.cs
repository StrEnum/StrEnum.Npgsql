using System.Text;

namespace StrEnum.Npgsql;

internal static class PostgresNaming
{
    /// <summary>
    /// Converts a PascalCase or camelCase identifier to snake_case, which is the conventional
    /// naming style for Postgres enum types.
    /// </summary>
    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (char.IsUpper(c) && i > 0 && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                builder.Append('_');

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
