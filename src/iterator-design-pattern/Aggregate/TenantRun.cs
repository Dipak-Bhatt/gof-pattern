using IteratorDesignPattern.Iterator;
using IteratorDesignPattern.Models;

namespace IteratorDesignPattern.Aggregate;

/// <summary>
/// The GoF Aggregate: it owns the tenants, knows the rotating order for a slot, and hands
/// out cursors. The backing array and the rotation index stay private.
/// </summary>
public sealed class TenantRun : ITenantAggregate
{
    private readonly Tenant[] _sorted;

    /// <param name="tenants">The live tenants; empty and repeated ids are dropped.</param>
    public TenantRun(IEnumerable<Tenant> tenants)
    {
        ArgumentNullException.ThrowIfNull(tenants);

        _sorted = tenants
            .Where(tenant => tenant.Id != Guid.Empty)
            .DistinctBy(tenant => tenant.Id)
            .OrderBy(tenant => tenant.Id)
            .ToArray();
    }

    /// <summary>How many tenants a full run would visit.</summary>
    public int Count => _sorted.Length;

    /// <summary>
    /// The order a run in <paramref name="slot"/> visits the tenants: sorted by id, then
    /// rotated so that <c>slot % Count</c> comes first. Over consecutive slots every
    /// tenant is first in turn.
    /// </summary>
    public IReadOnlyList<Tenant> Order(long slot)
    {
        if (_sorted.Length == 0)
            return _sorted;

        var start = (int)(((slot % _sorted.Length) + _sorted.Length) % _sorted.Length);

        return start == 0
            ? _sorted
            : [.. _sorted[start..], .. _sorted[..start]];
    }

    /// <inheritdoc />
    public ITenantIterator CreateIterator(
        long slot,
        TimeProvider time,
        long startedAt,
        TimeSpan budget,
        CancellationToken stopStarting) =>
        new TenantRunIterator(Order(slot), time, startedAt, budget, stopStarting);

    /// <summary>
    /// The same traversal written in the C# idiom. <c>yield return</c> compiles to an
    /// iterator state machine, so this is what most production code uses instead of a
    /// hand-written iterator class.
    /// </summary>
    public IEnumerable<Tenant> Startable(
        long slot,
        TimeProvider time,
        long startedAt,
        TimeSpan budget,
        CancellationToken stopStarting)
    {
        var ordered = Order(slot);

        for (var index = 0; index < ordered.Count; index++)
        {
            var outOfTime = index > 0 && time.GetElapsedTime(startedAt) >= budget;
            if (stopStarting.IsCancellationRequested || outOfTime)
                yield break;

            yield return ordered[index];
        }
    }
}
