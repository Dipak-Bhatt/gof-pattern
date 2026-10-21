using System.Collections.Immutable;

namespace FlyweightDesignPattern;

/// <summary>
/// Extrinsic state for one request: a shared <see cref="RlsShape"/> plus the values that fill it.
/// The names are intrinsic and live in the shape; only the values change per request.
/// </summary>
public sealed class RlsContext
{
    private readonly ImmutableArray<string> _values;

    public RlsContext(RlsShape shape, params string[] values)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != shape.Names.Count)
        {
            throw new ArgumentException(
                $"The shape has {shape.Names.Count} names but {values.Length} values were supplied.",
                nameof(values));
        }

        Shape = shape;
        _values = ImmutableArray.CreateRange(values);
    }

    public RlsShape Shape { get; }

    public IReadOnlyList<string> Values => _values;
}
