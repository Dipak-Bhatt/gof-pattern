namespace DecoratorDesignPattern;

/// <summary>
/// BASE DECORATOR — forwards to the wrapped sender. Every concrete decorator derives from here.
/// </summary>
public abstract class NotificationSenderDecorator(INotificationSender inner) : INotificationSender
{
    public INotificationSender Inner { get; } = inner;

    public virtual Task<bool> SendAsync(NotificationIntent intent) => Inner.SendAsync(intent);
}
