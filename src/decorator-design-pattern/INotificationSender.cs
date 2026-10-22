namespace DecoratorDesignPattern;

/// <summary>
/// COMPONENT — the only interface the caller knows. It answers false when an intent was refused
/// or not written, never throws for a business failure.
/// </summary>
public interface INotificationSender
{
    Task<bool> SendAsync(NotificationIntent intent);
}
