using System.Collections.Immutable;

namespace FlyweightDesignPattern;

/// <summary>
/// Extrinsic state: one request's values plus a reference to the shared
/// <see cref="CompiledPrefix"/>. This is the role <c>Tree</c> plays in the classic example —
/// the flyweight reference is shared, while the per-request values belong to this object.
/// </summary>
public sealed class RlsCommand
{
    private readonly ImmutableArray<string> _values;

    public RlsCommand(CompiledPrefix prefix, IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != prefix.ParameterNames.Length)
        {
            throw new ArgumentException(
                $"The prefix has {prefix.ParameterNames.Length} parameters but {values.Count} values were supplied.",
                nameof(values));
        }

        Prefix = prefix;
        _values = ImmutableArray.CreateRange(values);
    }

    public CompiledPrefix Prefix { get; }

    public string Sql => Prefix.Sql;

    public string Render() => Prefix.Render(_values);
}
