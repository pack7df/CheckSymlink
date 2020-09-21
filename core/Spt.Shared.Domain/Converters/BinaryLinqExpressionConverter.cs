using System.Linq.Expressions;
using Serialize.Linq.Serializers;

namespace Spt.Shared.Domain.Converters
{
    internal class BinaryLinqExpressionConverter
    {
        public static Expression ReadExpression(byte[] data)
        {
            var expressionSerializer = new ExpressionSerializer(new BinarySerializer());
            var expression = expressionSerializer.DeserializeBinary(data);
            return expression;
        }

        public static byte[] WriteExpression(Expression expression)
        {
            var expressionSerializer = new ExpressionSerializer(new BinarySerializer());
            return expressionSerializer.SerializeBinary(expression);
        }
    }
}
