using System.Globalization;
using Essential;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Essential.Diagnostics.Tests
{


    /// <summary>
    ///This is a test class for StringTemplateTest and is intended
    ///to contain all StringTemplateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringTemplateTest
    {
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void FormatAtStart()
        {
            var template = "{a}y";
            var arguments = new Dictionary<string, object>() { { "a", "A" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("Ay", actual);
        }

        [TestMethod()]
        public void FormatAtEnd()
        {
            var template = "x{a}";
            var arguments = new Dictionary<string, object>() { { "a", "A" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("xA", actual);
        }

        [TestMethod()]
        public void FormatInMiddle()
        {
            var template = "x{a}y";
            var arguments = new Dictionary<string, object>() { { "a", "A" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("xAy", actual);
        }

        [TestMethod()]
        public void RepeatFormat()
        {
            var template = "x{a}{a}y";
            var arguments = new Dictionary<string, object>() { { "a", "A" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("xAAy", actual);
        }

        [TestMethod()]
        public void MultipleFormat()
        {
            var template = "x{a}{b}y";
            var arguments = new Dictionary<string, object>() { { "a", "A" }, { "b", "B" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("xABy", actual);
        }

        [TestMethod()]
        public void SpecificFormatProvider()
        {
            var template = "{a:f1}";
            var arguments = new Dictionary<string, object>() { { "a", 1.0 } };

            var actual = StringTemplate.Format(CultureInfo.CreateSpecificCulture("de-DE"), template, arguments);

            Assert.AreEqual("1,0", actual);
        }

        [TestMethod()]
        public void EscapedCharacters()
        {
            var template = "{{{a}}}";
            var arguments = new Dictionary<string, object>() { { "a", "A" } };

            var actual = StringTemplate.Format(template, arguments);

            Assert.AreEqual("{A}", actual);
        }

        [TestMethod()]
        public void FormatFromDelegate()
        {
            var template = "{a}";

            int count = 0;
            string lastName = null;
            var actual = StringTemplate.Format(template,
                                               delegate(string name, out object value)
                                               {
                                                   count++;
                                                   lastName = name;
                                                   value = "A";
                                                   return true;
                                               });

            Assert.AreEqual("A", actual);
            Assert.AreEqual(1, count);
            Assert.AreEqual("a", lastName);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void MissingFormatEndException()
        {
            var template = "{";
            var arguments = new Dictionary<string, object>();

            var actual = StringTemplate.Format(template, arguments);

            Assert.Fail("Should have thrown exception.");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void MissingFormatStartException()
        {
            var template = "}";
            var arguments = new Dictionary<string, object>();

            var actual = StringTemplate.Format(template, arguments);

            Assert.Fail("Should have thrown exception.");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTemplateException()
        {
            var arguments = new Dictionary<string, object>();

            var actual = StringTemplate.Format(null, arguments);

            Assert.Fail("Should have thrown exception.");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgumentsException()
        {
            var template = "x";

            var actual = StringTemplate.Format(template, (IDictionary<string, object>)null);

            Assert.Fail("Should have thrown exception.");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullDelegateException()
        {
            var template = "x";

            var actual = StringTemplate.Format(template, (StringTemplate.GetValue)null);

            Assert.Fail("Should have thrown exception.");
        }
    }
}
