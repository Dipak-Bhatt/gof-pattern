using System.Collections.Immutable;
using System.Text;

namespace FlyweightDesignPattern;

/// <summary>
/// Intrinsic state: the ordered setting names that determine the SQL shape. Two contexts with the
/// same names share one <see cref="CompiledPrefix"/>.
/// </summary>
public sealed class RlsShape : IEquatable<RlsShape>
{
    private readonly ImmutableArray<string> _names;
    private readonly int _hashCode;

    private RlsShape(ImmutableArray<string> names, string key)
    {
        _names = names;
        Key = key;
        _hashCode = StringComparer.Ordinal.GetHashCode(key);
    }

    /// <summary>The setting names, in the order they appear in the SQL.</summary>
    public IReadOnlyList<string> Names => _names;

    /// <summary>A collision-free cache key for this shape.</summary>
    public string Key { get; }

    public static RlsShape Create(params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);
        if (names.Length == 0)
        {
            throw new ArgumentException("A shape needs at least one setting name.", nameof(names));
        }

        foreach (var name in names)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Setting names must not be null or empty.", nameof(names));
            }
        }

        var immutable = ImmutableArray.CreateRange(names);
        return new RlsShape(immutable, BuildKey(immutable));
    }

    public bool Equals(RlsShape? other) =>
        other is not null && string.Equals(Key, other.Key, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is RlsShape other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private static string BuildKey(ImmutableArray<string> names)
    {
        // Length-prefixed, so the key is unambiguous even when a name contains a separator.
        var builder = new StringBuilder();
        foreach (var name in names)
        {
            builder.Append(name.Length).Append(':').Append(name);
        }

        return builder.ToString();
    }
}
