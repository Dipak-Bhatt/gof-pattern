using IteratorDesignPattern.Models;

namespace IteratorDesignPattern.Iterator;

/// <summary>
/// The concrete Iterator. It owns the position and the stop rules: the first tenant always
/// starts, so a run makes progress; later tenants stop when the time budget is spent or the
/// caller cancels.
/// </summary>
public sealed class TenantRunIterator(
    IReadOnlyList<Tenant> ordered,
    TimeProvider time,
    long startedAt,
    TimeSpan budget,
    CancellationToken stopStarting) : ITenantIterator
{
    private int _position;

    public bool HasNext
    {
        get
        {
            if (_position >= ordered.Count)
                return false;

            if (stopStarting.IsCancellationRequested)
                return false;

            if (_position > 0 && time.GetElapsedTime(startedAt) >= budget)
                return false;

            return true;
        }
    }

    public Tenant Next()
    {
        if (!HasNext)
            throw new InvalidOperationException("No tenant is left to start.");

        return ordered[_position++];
    }

    public void Reset() => _position = 0;
}
