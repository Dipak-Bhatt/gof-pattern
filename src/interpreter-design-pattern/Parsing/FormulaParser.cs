using System.Globalization;

namespace InterpreterDesignPattern;

/// <summary>
/// Builds an expression tree from the formula text. Recursive descent keeps the grammar
/// visible: each parsing method below corresponds to one line of the grammar.
///
/// expression := term (("+" | "-") term)*
/// term       := primary (("*" | "/") primary)*
/// primary    := number | "{" name "}" | "(" expression ")" | primary "%"
/// </summary>
public sealed class FormulaParser
{
    private readonly string _text;
    private int _position;

    private FormulaParser(string text) => _text = text;

    /// <summary>Parses a complete formula into a ready-to-interpret expression tree.</summary>
    public static IExpression Parse(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            throw new FormulaException("A formula cannot be empty.");
        }

        var parser = new FormulaParser(formula);
        var expression = parser.ParseExpression();
        parser.SkipWhitespace();

        if (!parser.AtEnd)
        {
            throw new FormulaException(
                $"Unexpected character '{parser.Current}' at position {parser._position + 1}.");
        }

        return expression;
    }

    private bool AtEnd => _position >= _text.Length;

    private char Current => _text[_position];

    // Lowest precedence: addition and subtraction.
    private IExpression ParseExpression()
    {
        var left = ParseTerm();

        while (true)
        {
            SkipWhitespace();

            if (Match('+'))
            {
                left = new AddExpression(left, ParseTerm());
            }
            else if (Match('-'))
            {
                left = new SubtractExpression(left, ParseTerm());
            }
            else
            {
                return left;
            }
        }
    }

    // Higher precedence: multiplication and division.
    private IExpression ParseTerm()
    {
        var left = ParsePrimary();

        while (true)
        {
            SkipWhitespace();

            if (Match('*'))
            {
                left = new MultiplyExpression(left, ParsePrimary());
            }
            else if (Match('/'))
            {
                left = new DivideExpression(left, ParsePrimary());
            }
            else
            {
                return left;
            }
        }
    }

    // Highest precedence: a number, a variable, or a parenthesised expression.
    private IExpression ParsePrimary()
    {
        SkipWhitespace();

        if (AtEnd)
        {
            throw new FormulaException("The formula ends before a value was read.");
        }

        IExpression expression;

        if (Match('('))
        {
            expression = ParseExpression();

            if (!Match(')'))
            {
                throw new FormulaException("A '(' was opened but never closed.");
            }
        }
        else if (Current == '{')
        {
            expression = ParseVariable();
        }
        else if (char.IsAsciiDigit(Current) || Current == '.')
        {
            expression = ParseNumber();
        }
        else
        {
            throw new FormulaException(
                $"Unexpected character '{Current}' at position {_position + 1}.");
        }

        // A percentage is postfix and may follow a literal, a variable or a group.
        while (Match('%'))
        {
            expression = new PercentExpression(expression);
        }

        return expression;
    }

    private IExpression ParseVariable()
    {
        _position++; // Skip '{'.
        var start = _position;

        while (!AtEnd && Current != '}')
        {
            _position++;
        }

        if (AtEnd)
        {
            throw new FormulaException("A '{' was opened but never closed.");
        }

        var name = _text[start.._position].Trim();
        _position++; // Skip '}'.

        if (name.Length == 0)
        {
            throw new FormulaException("An empty {} does not name a value.");
        }

        return new VariableExpression(name);
    }

    private IExpression ParseNumber()
    {
        var start = _position;

        while (!AtEnd && (char.IsAsciiDigit(Current) || Current == '.'))
        {
            _position++;
        }

        var text = _text[start.._position];

        if (!decimal.TryParse(
                text,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var value))
        {
            throw new FormulaException($"'{text}' is not a valid number.");
        }

        return new NumberExpression(value);
    }

    private bool Match(char token)
    {
        SkipWhitespace();

        if (AtEnd || Current != token)
        {
            return false;
        }

        _position++;
        return true;
    }

    private void SkipWhitespace()
    {
        while (!AtEnd && char.IsWhiteSpace(Current))
        {
            _position++;
        }
    }
}
