namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — caches a read so the same id is not answered twice. It is a decorator,
/// not a new reader: it implements <see cref="IEmployeeReader"/> and delegates to the wrapped one
/// only on a miss.
/// </summary>
public sealed class CachingEmployeeReaderDecorator(IEmployeeReader inner) : EmployeeReaderDecorator(inner)
{
    private readonly Dictionary<int, Employee?> _cache = new();

    public override Task<Employee?> ReadAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"  [Cache] HIT  id={id} -> {cached?.Name ?? "not found"}");
            return Task.FromResult(cached);
        }

        Console.WriteLine($"  [Cache] MISS id={id} — asking the wrapped reader");
        var result = Inner.ReadAsync(id).GetAwaiter().GetResult();
        _cache[id] = result;
        return Task.FromResult(result);
    }
}
