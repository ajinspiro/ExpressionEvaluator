using System.Linq.Expressions;
using System.Reflection.Emit;


string exp = string.Empty;
Parser parser = null!;

exp = "5";
parser = new Parser(exp);
Console.WriteLine(parser.CallExpr());

exp = "1+2";
parser = new Parser(exp);
Console.WriteLine(parser.CallExpr());

exp = "-1+2";
parser = new Parser(exp);
Console.WriteLine(parser.CallExpr());

exp = "-(1+(2))";
parser = new Parser(exp);
Console.WriteLine(parser.CallExpr());

public enum TOKEN
{
    IllegalToken,
    TOK_PLUS,
    TOK_MINUS,
    TOK_STAR,
    TOK_SLASH,
    TOK_DOUBLE,
    TOK_OPAREN,
    TOK_CPAREN,
    TOK_NULL
}

public class Parser(string Expr) : Lexer(Expr)
{
    ILGenerator ILGenerator;
    DynamicMethod expressionEvaluator = new DynamicMethod(
        nameof(expressionEvaluator), typeof(double), null, typeof(Parser), false
        );
    TOKEN CurrentToken;
     
    public double CallExpr()
    {
        ILGenerator = expressionEvaluator.GetILGenerator();
        CurrentToken = GetTOKEN(); 
        ILGenerator.Emit(OpCodes.Ret);
        var result = Expression.Lambda(Expr()).Compile().DynamicInvoke();
        return (double)result;

        double? val = (double?)expressionEvaluator.Invoke(null, null);
        return val is null ? throw new Exception("val is null") : val.Value;
    }

    public Expression Expr()
    {
        Expression leftExp = Term();
        if (CurrentToken == TOKEN.TOK_PLUS || CurrentToken == TOKEN.TOK_MINUS)
        {
            TOKEN leftToken = CurrentToken;
            CurrentToken = GetTOKEN();
            Expression rightExp = Expr();
            ILGenerator.Emit(leftToken == TOKEN.TOK_PLUS ? OpCodes.Add : OpCodes.Sub);
            if (leftToken == TOKEN.TOK_PLUS)
                return Expression.Add(leftExp, rightExp);
            else
                return Expression.Subtract(leftExp, rightExp);
        }
        return leftExp;
    }

    public Expression Term()
    {
        Expression leftExp = Factor();
        if (CurrentToken == TOKEN.TOK_STAR || CurrentToken == TOKEN.TOK_SLASH)
        {
            TOKEN leftToken = CurrentToken;
            CurrentToken = GetTOKEN();
            Expression rightExp = Term();
            ILGenerator.Emit(leftToken == TOKEN.TOK_STAR ? OpCodes.Mul : OpCodes.Div);
            if (leftToken == TOKEN.TOK_STAR)
                return Expression.Multiply(leftExp, rightExp);
            else
                return Expression.Divide(leftExp, rightExp);
        }
        return leftExp;
    }

    public Expression Factor()
    {
        if (CurrentToken == TOKEN.TOK_DOUBLE)
        {
            ILGenerator.Emit(OpCodes.Ldc_R8, Number);

            CurrentToken = GetTOKEN();
            return Expression.Constant(Number);
        }
        else if (CurrentToken == TOKEN.TOK_OPAREN)
        {
            CurrentToken = GetTOKEN();
            Expression rightExp = Expr();
            if (CurrentToken != TOKEN.TOK_CPAREN)
                throw new Exception("Missing c_paren");
            CurrentToken = GetTOKEN();
            return rightExp;
        }
        else if (CurrentToken == TOKEN.TOK_PLUS || CurrentToken == TOKEN.TOK_MINUS)
        {
            TOKEN leftToken = CurrentToken;
            CurrentToken = GetTOKEN();
            Expression rightExp = Factor();
            if (leftToken == TOKEN.TOK_MINUS)
            {
                ILGenerator.Emit(OpCodes.Neg);
                return Expression.Negate(rightExp);
            }
            return rightExp;
        }
        else
        {
            throw new Exception("Error parsing expression");
        }
    }
}

public class Lexer(string Expr)
{
    private int index = 0;
    private int length = Expr.Length;

    public double Number { get; private set; }
    public TOKEN GetTOKEN()
    {
        while (index < length && (Expr[index] == '\t' || Expr[index] == ' '))
        {
            index++;
        }

        if (index == length)
        {
            return TOKEN.TOK_NULL;
        }

        switch (Expr[index])
        {
            case '+':
                index++;
                return TOKEN.TOK_PLUS;
            case '-':
                index++;
                return TOKEN.TOK_MINUS;
            case '*':
                index++;
                return TOKEN.TOK_STAR;
            case '/':
                index++;
                return TOKEN.TOK_SLASH;
            case '(':
                index++;
                return TOKEN.TOK_OPAREN;
            case ')':
                index++;
                return TOKEN.TOK_CPAREN;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                {
                    string dbl = string.Empty;
                    while (index < length && int.TryParse(Expr[index].ToString(), out int digit))
                    {
                        dbl = dbl + Expr[index];
                        index++;
                    }
                    Number = double.Parse(dbl);
                    return TOKEN.TOK_DOUBLE;
                }
            default:
                throw new Exception("Invalid character in expression");
        }
    }
}
