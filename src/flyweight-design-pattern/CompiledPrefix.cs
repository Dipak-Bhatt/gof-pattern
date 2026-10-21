using System.Collections.Immutable;
using System.Text;

namespace FlyweightDesignPattern;

/// <summary>
/// Intrinsic state: immutable, shared, and expensive to build. One instance serves every request
/// that has the same ordered set of setting names. <see cref="Render"/> receives the extrinsic
/// values, just as the classic <c>TreeType.Draw(x, y)</c> receives its coordinates.
/// </summary>
public sealed class CompiledPrefix
{
    public CompiledPrefix(string sql, ImmutableArray<string> parameterNames)
    {
        Sql = sql;
        ParameterNames = parameterNames;
    }

    public string Sql { get; }

    public ImmutableArray<string> ParameterNames { get; }

    /// <summary>
    /// Binds the per-request values to this shared prefix. The prefix is reused; only the
    /// parameter values differ.
    /// </summary>
    public string Render(IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count != ParameterNames.Length)
        {
            throw new ArgumentException(
                $"The prefix has {ParameterNames.Length} parameters but {values.Count} values were supplied.",
                nameof(values));
        }

        var builder = new StringBuilder(Sql.Length + (values.Count * 32));
        builder.Append(Sql).Append("  -- ");

        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder
                .Append('@')
                .Append(ParameterNames[i])
                .Append("='")
                .Append(values[i].Replace("'", "''"))
                .Append('\'');
        }

        return builder.ToString();
    }
}
