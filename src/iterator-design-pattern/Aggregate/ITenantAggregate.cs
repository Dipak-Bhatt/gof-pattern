using IteratorDesignPattern.Iterator;

namespace IteratorDesignPattern.Aggregate;

/// <summary>
/// The GoF Aggregate role: it creates an iterator over its tenants but says nothing
/// about how they are stored or ordered internally.
/// </summary>
public interface ITenantAggregate
{
    /// <summary>Creates a cursor over the order a run in <paramref name="slot"/> visits.</summary>
    ITenantIterator CreateIterator(
        long slot,
        TimeProvider time,
        long startedAt,
        TimeSpan budget,
        CancellationToken stopStarting);
}
