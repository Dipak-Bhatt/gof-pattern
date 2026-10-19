namespace DecoratorDesignPattern;

/// <summary>
/// CONCRETE DECORATOR — counts what was sent and refused, the cross-cutting metric a caller wants
/// without the component knowing about metrics.
/// </summary>
public sealed class MetricsNotificationSenderDecorator(INotificationSender inner) : NotificationSenderDecorator(inner)
{
    public int Sent { get; private set; }
    public int Refused { get; private set; }

    public override async Task<bool> SendAsync(NotificationIntent intent)
    {
        var sent = await Inner.SendAsync(intent).ConfigureAwait(false);
        if (sent) Sent++; else Refused++;
        return sent;
    }
}
