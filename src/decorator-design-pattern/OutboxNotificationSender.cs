namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE COMPONENT — writes the intent to <c>workhorse_outbox</c> in <c>rigo_hr</c> (ADR-0012).
/// Fails the first time for an intent to make the retry decorator visible.
/// </summary>
public sealed class OutboxNotificationSender : INotificationSender
{
    private readonly HashSet<string> _attempted = new();

    public Task<bool> SendAsync(NotificationIntent intent)
    {
        Console.WriteLine($"    [outbox] INSERT INTO workhorse_outbox (type, key) VALUES ('{intent.Type}', '{intent.IdempotencyKey}')");

        // A transient failure on the very first attempt, the way a dropped connection behaves:
        // the retry decorator's second call is answered.
        var firstAttempt = _attempted.Add(intent.IdempotencyKey);
        return Task.FromResult(!firstAttempt);
    }
}
