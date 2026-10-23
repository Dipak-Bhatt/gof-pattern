namespace InterpreterDesignPattern;

/// <summary>
/// The context holds the values that variables resolve to for one interpretation.
/// Reusing the same tree with a different context evaluates the same formula for
/// another employee or period.
/// </summary>
public sealed class 
    FormulaContext
{
    private readonly Dictionary<string, decimal> _values = new(StringComparer.OrdinalIgnoreCase);

    public FormulaContext(params (string Name, decimal Value)[] values)
    {
        foreach (var (name, value) in values)
        {
            _values[Normalize(name)] = value;
        }
    }

    public decimal Resolve(string name)
    {
        if (!_values.TryGetValue(Normalize(name), out var value))
        {
            throw new FormulaException(
                $"The formula uses '{name}', but no value was supplied for it.");
        }

        return value;
    }

    // "Basic Salary" and "BasicSalary" are treated as the same variable.
    private static string Normalize(string name) => name.Replace(" ", string.Empty);
}
