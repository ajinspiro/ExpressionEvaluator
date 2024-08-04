using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace ConsoleAppDEL;
internal class ExprEvaluator
{
    public static void Main()
    {
        var three = Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1), Expression.Constant(2));
        Console.WriteLine($"1+2={ExpressionEvaluator.Evaluate(three)}");

        var six =
            Expression.MakeBinary(ExpressionType.Add, Expression.Constant(1),
            Expression.MakeBinary(
            ExpressionType.Multiply, Expression.Constant(2), Expression.Constant(3)
            )
            );
        Console.WriteLine($"1+(2*3)={ExpressionEvaluator.Evaluate(six)}");

    }

    public class ExpressionEvaluator
    {
        public static int Evaluate(Expression expression)
        {
            var visitor = new ExprVisitor();
            var output = (ConstantExpression)visitor.Visit(expression) ?? throw new Exception("Not possible");
            return Expression.Lambda<Func<int>>(new ExprVisitor().Visit(output)).Compile()();
        }
    }

    public class ExprVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            if (node is null) throw new Exception();

            if (node is ConstantExpression)
                return base.Visit(node);

            if (node is BinaryExpression binNode)
            {
                Expression left = Visit(binNode.Left);
                Expression right = Visit(binNode.Right);
                int value = Expression.Lambda<Func<int>>(Expression.MakeBinary(binNode.NodeType, left, right)).Compile()();
                return Expression.Constant(value);
            }

            if (node is UnaryExpression unNode)
            {
                Expression exp = Visit(unNode.Operand);
                int value = Expression.Lambda<Func<int>>(Expression.MakeUnary(unNode.NodeType, exp, null!)).Compile()();
                return Expression.Constant(value);
            }

            throw new Exception();
        }
    }
}
