namespace InterpreterDesignPattern;

/// <summary>
/// Raised when a formula is malformed or a value is missing from the context.
/// </summary>
public sealed class FormulaException(string message) : Exception(message);
