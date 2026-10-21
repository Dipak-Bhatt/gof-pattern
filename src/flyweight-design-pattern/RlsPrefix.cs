using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

namespace FlyweightDesignPattern;

/// <summary>
/// The flyweight factory. Given an <see cref="RlsShape"/> it returns the shared
/// <see cref="CompiledPrefix"/> for that shape; <see cref="Compile"/> pairs the prefix with the
/// per-request values. The factory is an instance, like the classic <c>TreeFactory</c>, so its
/// cache is scoped and testable.
/// </summary>
public sealed class RlsPrefix
{
    // Keyed on the shape, which is fixed per product: at most a handful of entries for the
    // factory's lifetime, no matter how many requests pass through.
    private readonly ConcurrentDictionary<string, CompiledPrefix> _compiled =
        new(StringComparer.Ordinal);

    /// <summary>How many intrinsic objects have been created; useful in the demo and in tests.</summary>
    public int SharedObjectCount => _compiled.Count;

    /// <summary>The shared prefix for this shape.</summary>
    public CompiledPrefix Resolve(RlsShape
        shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        return _compiled.GetOrAdd(shape.Key, static (_, value) => Build(value), shape);
    }

    /// <summary>The SQL for this shape, without binding any values.</summary>
    public string SqlFor(RlsShape shape) => Resolve(shape).Sql;

    /// <summary>
    /// Pairs the shared prefix with one request's values. The prefix is reused; only the values
    /// in the returned <see cref="RlsCommand"/> differ.
    /// </summary>
    public RlsCommand Compile(RlsContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new RlsCommand(Resolve(context.Shape), context.Values);
    }

    private static CompiledPrefix Build(RlsShape shape)
    {
        var names = shape.Names;
        var parameterNames = ImmutableArray.CreateBuilder<string>(names.Count);
        var sql = new StringBuilder("SELECT ");

        for (var i = 0; i < names.Count; i++)
        {
            if (i > 0)
            {
                sql.Append(", ");
            }

            var parameterName = $"__rls_{Sanitise(names[i])}_{i}";
            parameterNames.Add(parameterName);

            sql
                .Append("set_config('")
                .Append(names[i])
                .Append("', @")
                .Append(parameterName)
                .Append(", true)");
        }

        sql.Append(';');
        return new CompiledPrefix(sql.ToString(), parameterNames.ToImmutable());
    }

    private static string Sanitise(string name)
    {
        var characters = name.ToCharArray();
        for (var i = 0; i < characters.Length; i++)
        {
            if (!char.IsLetterOrDigit(characters[i]) && characters[i] != '_')
            {
                characters[i] = '_';
            }
        }

        return new string(characters);
    }
}
