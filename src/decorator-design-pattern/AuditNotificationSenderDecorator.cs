namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — writes an audit line. It logs the type and idempotency key only, never an
/// address, exactly as <c>HrNotifyLog</c> does (event ids 7201–7204, docs/standards/logging.md).
/// </summary>
public sealed class AuditNotificationSenderDecorator(INotificationSender inner) : NotificationSenderDecorator(inner)
{
    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        Console.WriteLine($"  [Audit] sending '{intent.Type}' ({intent.IdempotencyKey})");
        var sent = await Inner.SendAsync(intent).ConfigureAwait(false);
        Console.WriteLine($"  [Audit] {(sent ? "sent" : "refused")} '{intent.Type}' ({intent.IdempotencyKey})");
        return sent;
    }
}
