using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Essential.Diagnostics.Tests
{
    [TestClass]
    public class ExpressionParserTests
    {
        [TestMethod]
        public void ValidEqualityExpression()
        {
            var parser = new ExpressionParser<Func<bool>>();

            string expression = "1 == 1";
            Func<bool> compiled = parser.Parse(expression).Compile();

            Assert.IsTrue(compiled());
    }

        [TestMethod]
        public void ValidEqualityExpressionWithParameter()
        {
            var parser = new ExpressionParser<Func<int, bool>>("x");

            string expression = "x == 1";
            Func<int,bool> compiled = parser.Parse(expression).Compile();

            Assert.IsTrue(compiled(1));
            Assert.IsFalse(compiled(2));
        }

        public void Stuff()
        {
            Microsoft.CSharp.CSharpCodeProvider csprovider = new Microsoft.CSharp.CSharpCodeProvider();
            var options = new CompilerParameters();
            var source = @"using System;
public static class Class1 {
  public static int Func(int a, int b, int c, int d) {
    return a + b * c + d;
  }
}";
            var result = csprovider.CompileAssemblyFromSource(options, source);
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

            var answer = class1.InvokeMember("Func", BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, null, null, new object[] { 1, 2, 3, 4 });
            Console.WriteLine(answer);
        }

    }
}
