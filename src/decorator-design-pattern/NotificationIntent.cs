namespace DecoratorDesignPattern;

/// <summary>
/// The thing a sender is asked to hand on. Mirrors <c>NotificationIntent</c> in
/// HR.Services/Notify/HrNotifications.cs: a type and an idempotency key, never an address.
/// </summary>
public record NotificationIntent(string Type, string IdempotencyKey);
