using System.Collections.Concurrent;
using System.Linq;

namespace StrEnum.Npgsql;

/// <summary>
/// Extracts Postgres enum labels from a <see cref="StringEnum{TEnum}"/> type.
/// </summary>
internal static class StringEnumLabels
{
    private static readonly ConcurrentDictionary<Type, string[]> Cache = new();

    /// <summary>
    /// Returns the underlying string values of all members defined on <typeparamref name="TEnum"/>,
    /// in the order they were declared. These values are used as labels of the Postgres enum.
    /// </summary>
    public static string[] For<TEnum>() where TEnum : StringEnum<TEnum>, new()
    {
        return Cache.GetOrAdd(typeof(TEnum), _ => StringEnum<TEnum>.GetMembers().Select(m => (string)m).ToArray());
    }
}
