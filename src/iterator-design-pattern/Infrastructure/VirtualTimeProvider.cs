namespace IteratorDesignPattern.Infrastructure;

/// <summary>A clock the demo advances by hand, so time-budget behaviour is deterministic.</summary>
public sealed class VirtualTimeProvider : TimeProvider
{
    private long _ticks;
    public override long GetTimestamp() => _ticks;

    public void Advance(TimeSpan by) => _ticks += by.Ticks;
}
