namespace SingletonDesignPattern.Domain;

/// <summary>
/// The simplest possible Singleton — a good place to learn the pattern.
///
/// A logger that every part of the app can use, but there is only ONE.
/// The whole pattern is three small ideas:
///   1. a private static field that holds the one instance,
///   2. a private constructor so nobody outside can write <c>new SimpleLogger()</c>,
///   3. one public <c>Instance</c> property that creates it once and returns it.
/// </summary>
public sealed class SimpleLogger
{
    // Step 1: one private static field holds the single instance.
    // It starts out null — the instance is created only when first needed.
    private static SimpleLogger? _instance;

    // Step 2: a private constructor, so nobody outside can write `new SimpleLogger()`.
    private SimpleLogger()
    {
        Messages = new List<string>();
    }

    // Step 3: the one public way to get it — create it if needed, then return it.
    public static SimpleLogger Instance
    {
        get
        {
            _instance ??= new SimpleLogger();
            return _instance;
        }
    }

    /// <summary>Every message written through the one logger.</summary>
    public List<string> Messages { get; }

    public void Log(string message)
    {
        Messages.Add(message);
        Console.WriteLine($"  [LOG] {message}");
    }
}
