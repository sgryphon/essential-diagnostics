using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;

namespace Essential
{
    public class ExpressionParser<T>
    {
        private ParameterExpression[] parameters;
        private Type returnType;

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
            returnType = argTypes[argTypes.Length - 1];
        }

        public Expression<T> Parse(string expression)
        {
            Expression parsed = ParseExpression(expression);
            Expression<T> typed = parsed as Expression<T>;
            return typed;
        }

        private Expression ParseExpression(string expression)
        {
            var source = new StringBuilder();
            source.AppendLine("using System;");
            source.AppendLine("using System.Diagnostics;");
            source.AppendLine("using System.Linq.Expressions;");
            source.AppendLine("using System.Text.RegularExpressions;");

            source.AppendLine("namespace Test {");
            source.AppendLine("  public static class Class1 {");

            source.AppendLine("    public static object CreateExpression() {");

            // Expression<Func<decimal, int, bool>> exp1 = (x, y) => (x == y);
            var parameterTypeJoined = string.Join(",", parameters.Select(p => p.Type.FullName).ToArray());
            if (parameterTypeJoined.Length > 0)
            {
                parameterTypeJoined += ",";
            }
            parameterTypeJoined += returnType.FullName;
            var parameterNameJoined = string.Join(",", parameters.Select(p => p.Name).ToArray());
            var expressionStatement = string.Format("      Expression<Func<{0}>> expression = ({1}) => {2};",
                parameterTypeJoined, parameterNameJoined, expression);
            source.AppendLine(expressionStatement);

            source.AppendLine("      return expression;");
            source.AppendLine("    }"); // End of method
            source.AppendLine("  }"); // End of class
            source.AppendLine("}"); // End of namespace

            // TODO: Get rid of all this debug code (throw an exception or something if it fails).
            Console.WriteLine(source);

            var csprovider = new CSharpCodeProvider();
            var options = new CompilerParameters()
            {
                GenerateInMemory = true
            };
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            var result = csprovider.CompileAssemblyFromSource(options, source.ToString());

            Console.WriteLine("Compiler return: {0}", result.NativeCompilerReturnValue);
            foreach (string line in result.Output)
            {
                Console.WriteLine(line);
            }

            var assembly = result.CompiledAssembly;

            Console.WriteLine("Types:");
            foreach (var type in assembly.GetTypes())
            {
                Console.WriteLine(type.FullName);
            }

            var class1 = assembly.GetType("Test.Class1");

            Console.WriteLine("Methods:");
            foreach (var method in class1.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                Console.WriteLine(method.Name);
            }

            var answer = class1.InvokeMember("CreateExpression", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, null, null, null);
            var compiled = answer as Expression;
            return compiled;
        }
    }
}
