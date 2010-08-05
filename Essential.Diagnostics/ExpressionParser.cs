using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Essential
{
    public class ExpressionParser<T>
    {
        private ParameterExpression[] parameters;

        public ExpressionParser(params string[] parameterNames)
        {
            Type type = typeof(T);
            if (!type.IsSubclassOf(typeof(Delegate))) throw new ArgumentException("Expression type must be a delegate.", "T");

            var argTypes = type.GetGenericArguments();

            if (parameterNames.Length != (argTypes.Length - 1)) throw new ArgumentException("Parameter names must be the same length as the delegate parameters.");

            var parameterList = new List<ParameterExpression>();
            for (int index = 0; index < parameterNames.Length; index++)
            {
                var parameter = Expression.Parameter(argTypes[index], parameterNames[index]);
                parameterList.Add(parameter);
            }
            parameters = parameterList.ToArray();
        }

        public Expression<T> Parse(string expression)
        {
            var tokens = SplitTokens(expression);
            Expression body = ParseExpression(tokens);
            Expression<T> parsed = Expression.Lambda<T>(body, parameters);
            return parsed;
            //Predicate<T1> compiled = lambda.Compile();
            //return compiled;
        }

        private string[] SplitTokens(string expression)
        {
            var tokens = expression.Split(' ');
            return tokens;
        }

        private Expression ParseExpression(string[] tokens)
        {
            if (tokens.Length == 1)
            {
                var token = tokens[0];
                int value;
                if (int.TryParse(token, out value))
                {
                    return CreateConstant(value);
                }
                else
                {
                    return CreateParameter(token);
                }
            }
            else if (tokens.Length == 3)
            {
                var left = new string[] { tokens[0] };
                var operation = tokens[1];
                var right = new string[] {tokens[2] };

                ExpressionType binaryType;
                switch (operation)
                {
                    case "==":
                        binaryType = ExpressionType.Equal;
                        break;
                    default:
                        throw new Exception("Parse failed.");
                }
                return CreateBinary(binaryType, left, right);
            }
            else
            {
                throw new Exception("Wrong token length.");
            }
        }

        private BinaryExpression CreateBinary(ExpressionType binaryType, string[] left, string[] right)
        {
            Expression leftExpression = ParseExpression(left);

            Expression rightExpression = ParseExpression(right);

            BinaryExpression binary = Expression.MakeBinary(binaryType, leftExpression, rightExpression);
            return binary;
        }

        private ConstantExpression CreateConstant(object value)
        {
            return Expression.Constant(value);
        }

        private Expression CreateParameter(string parameterName)
        {
            return parameters.First(p => p.Name == parameterName);
        }


    }
}
