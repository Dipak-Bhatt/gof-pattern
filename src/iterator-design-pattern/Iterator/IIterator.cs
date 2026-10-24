namespace IteratorDesignPattern.Iterator;

/// <summary>
/// The GoF Iterator role: a cursor that walks a collection one element at a time.
/// The traversal state lives here, not in the collection and not in the client.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public interface IIterator<out T>
{
    /// <summary>Whether another element is available.</summary>
    bool HasNext { get; }

    /// <summary>Returns the current element and advances the cursor.</summary>
    /// <exception cref="InvalidOperationException">No element is left.</exception>
    T Next();

    /// <summary>Returns the cursor to the first element.</summary>
    void Reset();
}
