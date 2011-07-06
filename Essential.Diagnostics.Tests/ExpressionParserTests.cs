//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Xml.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.CodeDom.Compiler;
//using System.Reflection;

//namespace Essential.Diagnostics.Tests
//{
//    [TestClass]
//    public class ExpressionParserTests
//    {
//        [TestMethod]
//        public void ValidEqualityExpression()
//        {
//            var parser = new ExpressionParser<Func<bool>>();

//            string expression = "1 == 1";
//            Func<bool> compiled = parser.Parse(expression).Compile();

//            Assert.IsTrue(compiled());
//        }

//        [TestMethod]
//        public void ValidEqualityExpressionWithParameter()
//        {
//            var parser = new ExpressionParser<Func<int, bool>>("x");

//            string expression = "x == 1";
//            Func<int,bool> compiled = parser.Parse(expression).Compile();

//            Assert.IsTrue(compiled(1));
//            Assert.IsFalse(compiled(2));
//        }

//        [TestMethod]
//        public void ValidStringMethodWithParameter()
//        {
//            var parser = new ExpressionParser<Func<string, string, bool>>("a", "b");

//            string expression = "a.Length == b.Length";
//            Func<string,string,bool> compiled = parser.Parse(expression).Compile();

//            Assert.IsTrue(compiled("x", "y"));
//            Assert.IsFalse(compiled("x", "yz"));
//        }

//        [TestMethod]
//        public void StringReturnType()
//        {
//            var parser = new ExpressionParser<Func<string, int, string>>("s", "i");

//            string expression = "s.Substring(i,1)";
//            var compiled = parser.Parse(expression).Compile();

//            Assert.AreEqual("c", compiled("abcd", 2));
//        }

//    }
//}
