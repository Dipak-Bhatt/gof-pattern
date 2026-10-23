namespace InterpreterDesignPattern;

/// <summary>
/// The abstract expression. Every node in a formula implements this single operation,
/// so a caller can interpret a whole tree without knowing any concrete node type.
/// </summary>
public interface IExpression
{
    decimal Interpret(FormulaContext context);
}
