namespace PrototypeDesignPattern;

/// <summary>
/// PROTOTYPE — the single operation a client uses to copy an object without knowing its concrete
/// type or the shape of the object graph hanging off it.
/// </summary>
/// <typeparam name="T">The type the prototype returns; normally the concrete class itself.</typeparam>
public interface IPrototype<out T>
{
    /// <summary>
    /// Returns an independent copy. For an entity the copy carries a new identity (a zero id), so
    /// it can be saved as a new record.
    /// </summary>
    T Clone();
}
