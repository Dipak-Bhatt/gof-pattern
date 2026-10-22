namespace DecoratorDesignPattern;

/// <summary>
/// The thing a sender is asked to hand on: a type and an idempotency key, never an address.
/// </summary>
public record NotificationIntent(string Type, string IdempotencyKey);
