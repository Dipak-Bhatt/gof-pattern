using InterpreterDesignPattern;

Console.WriteLine("=== Interpreter Design Pattern — Allowance Formula ===\n");
Console.WriteLine("CONTEXT        FormulaContext              values supplied for one employee");
Console.WriteLine("ABSTRACT EXPR  IExpression.Interpret(ctx) evaluates one node");
Console.WriteLine("TERMINAL       NumberExpression, VariableExpression");
Console.WriteLine("NONTERMINAL    Add, Subtract, Multiply, Divide, Percent");
Console.WriteLine("PARSER         FormulaParser.Parse(text)   builds the expression tree\n");

const string formulaText = "{Basic Salary}/{Monthly Hours}*{Units}*150%";

var ram = new FormulaContext(
    ("Basic Salary", 20_800m),
    ("Monthly Hours", 208m),
    ("Units", 10m));

var formula = FormulaParser.Parse(formulaText);

Console.WriteLine($"Formula: {formulaText}");
Console.WriteLine("Parsed tree:");
PrintTree(formula);

Console.WriteLine($"\nRam's allowance: {formula.Interpret(ram):N2}");
Console.WriteLine("  (20,800 / 208) * 10 * 150% = 1,500.00\n");

var sita = new FormulaContext(
    ("Basic Salary", 31_200m),
    ("Monthly Hours", 208m),
    ("Units", 14m));

Console.WriteLine("Same tree, interpreted for Sita:");
Console.WriteLine($"  Sita's allowance: {formula.Interpret(sita):N2}\n");

Console.WriteLine("Parentheses change the tree, so they change the result:");
foreach (var text in new[] { "{Units} + {Monthly Hours} * 2", "({Units} + {Monthly Hours}) * 2" })
{
    var parsed = FormulaParser.Parse(text);
    Console.WriteLine($"  {text,-40} = {parsed.Interpret(ram),8:N2}");
}

Console.WriteLine("\nMalformed input is reported where the tree is built or interpreted:");
try
{
    FormulaParser.Parse("{Basic Salary} * {Bonus}").Interpret(ram);
}
catch (FormulaException ex)
{
    Console.WriteLine($"  missing value:      {ex.Message}");
}

try
{
    FormulaParser.Parse("{Basic Salary} / 0").Interpret(ram);
}
catch (FormulaException ex)
{
    Console.WriteLine($"  invalid arithmetic: {ex.Message}");
}

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Each grammar rule is its own class. A tree of those objects is the formula, and");
Console.WriteLine("Interpret() walks it with the supplied context. Adding an operator or a function");
Console.WriteLine("means adding a class, not editing one large evaluate() switch.");

static void PrintTree(IExpression expression, string indent = "")
{
    switch (expression)
    {
        case BinaryExpression binary:
            Console.WriteLine($"{indent}{binary.Operator}");
            PrintTree(binary.Left, indent + "  ");
            PrintTree(binary.Right, indent + "  ");
            break;

        case PercentExpression percent:
            Console.WriteLine($"{indent}%");
            PrintTree(percent.Operand, indent + "  ");
            break;

        default:
            Console.WriteLine($"{indent}{expression}");
            break;
    }
}
