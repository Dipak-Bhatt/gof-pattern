namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — retries a refused intent. The outbox already gives durability and replay;
/// this is the in-process "try again now" layer for a transient failure.
/// </summary>
public sealed class RetryingNotificationSenderDecorator(INotificationSender inner, int attempts = 3)
    : NotificationSenderDecorator(inner)
{
    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        for (var attempt = 1; ; attempt++)
        {
            if (await Inner.SendAsync(intent).ConfigureAwait(false))
                return true;

            if (attempt >= attempts)
                return false;

            Console.WriteLine($"  [Retry] attempt {attempt} refused '{intent.Type}' ({intent.IdempotencyKey}) — retrying");
        }
    }
}
