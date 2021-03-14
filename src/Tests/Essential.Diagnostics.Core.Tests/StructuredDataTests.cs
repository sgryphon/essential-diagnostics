using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class StructuredDataTests
    {
        [TestMethod()]
        public void StructuredPropertyValue()
        {
            var template = "x{a}";

            IStructuredData data = new StructuredData(template, "A");
            IDictionary<string, object> actual = data;

            Assert.AreEqual(1 + 1, actual.Count);
            Assert.AreEqual("a", actual.Keys.First());
            Assert.AreEqual("A", actual["a"]);
        }

        [TestMethod()]
        public void StructuredPropertyValueInOrder()
        {
            var template = "{b}x{a}";
            var a = "A";
            var b = "B";

            IStructuredData data = new StructuredData(template, a, b);
            IDictionary<string, object> actual = data;

            Assert.AreEqual(2 + 1, actual.Count);
            Assert.AreEqual("A", actual["b"]);
            Assert.AreEqual("B", actual["a"]);
        }

        [TestMethod()]
        public void StructuredPropertyWithException()
        {
            var template = "x{a}";

            IStructuredData data;
            try
            {
                throw new ApplicationException("B");
            }
            catch (Exception ex)
            {
                data = new StructuredData(ex, template, "A");
            }
            IDictionary<string, object> actual = data;

            Assert.AreEqual(2 + 1, actual.Count);
            Assert.AreEqual("A", actual["a"]);
            Assert.AreEqual("B", ((Exception)actual["Exception"]).Message);
        }

        [TestMethod()]
        public void StructuredPropertyWithAdditionalData()
        {
            var template = "x{a}";
            var additional = new Dictionary<string, object>() { { "b", "B" } };

            IStructuredData data = new StructuredData(additional, template, "A");
            IDictionary<string, object> actual = data;

            Assert.AreEqual(2 + 1, actual.Count);
            Assert.AreEqual("A", actual["a"]);
            Assert.AreEqual("B", actual["b"]);
        }

        [TestMethod()]
        public void StructuredStringFormatProperty()
        {
            var template = "x{a}";
            var properties = new Dictionary<string, object>() {
                { "a", 1 },
            };                  

            IStructuredData data = new StructuredData(properties, template);
            var actual = data.ToString();

            Assert.AreEqual("x1", actual);
        }

        [TestMethod()]
        public void StructuredStringFormatValue()
        {
            var template = "x{a}";

            IStructuredData data = new StructuredData(template, 1);
            var actual = data.ToString();

            Assert.AreEqual("x1", actual);
        }

        [TestMethod()]
        public void StructuredStringValuesInOrder()
        {
            var template = "{b}x{a}";
            var a = "A";
            var b = "B";

            IStructuredData data = new StructuredData(template, a, b);
            var actual = data.ToString();

            Assert.AreEqual("AxB", actual);
        }

        [TestMethod()]
        public void StructuredStringValuesMissingAreBlank()
        {
            var template = "{a}x{b}y";
            var a = "A";

            IStructuredData data = new StructuredData(template, a);
            var actual = data.ToString();

            Assert.AreEqual("Ax{b}y", actual);
        }

        [TestMethod()]
        public void StructuredStringValuesOverrideProperties()
        {
            var template = "{a}x{b}";
            var a = "A";

            var properties = new Dictionary<string, object>() {
                { "a", 1 },
                { "b", 2 },
            };

            IStructuredData data = new StructuredData(properties, template, a);
            var actual = data.ToString();

            Assert.AreEqual("Ax2", actual);
        }

        [TestMethod()]
        public void StructuredStringPropertyOnly()
        {
            var properties = new Dictionary<string, object>() {
                { "a", 1 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=1", actual);
        }

        [TestMethod()]
        public void StructuredDictionaryInitializerSyntaxAdd()
        {
            var data = new StructuredData
            {
                { "a", 1 },
            };

            var actual = data.ToString();

            Assert.AreEqual("a=1", actual);
        }


        [TestMethod()]
        public void StructuredDictionaryInitializerSyntaxIndexer()
        {
            var data = new StructuredData
            {
                ["a"] = 1,
            };

            var actual = data.ToString();

            Assert.AreEqual("a=1", actual);
        }

        [TestMethod()]
        public void StructuredAddShouldFailAfterInitialSetup()
        {
            var data = new StructuredData();
            data.Add("a", 1);
            var count = data.Count;

            Exception caught = null;
            try
            {
                data.Add("b", 2);
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }
            Assert.AreEqual(1, count);
            Assert.IsNotNull(caught);
        }

        [TestMethod()]
        public void StructuredIndexerShouldFailAfterInitialSetup()
        {
            var data = new StructuredData();
            data["a"] = 1;
            var count = data.Count;

            Exception caught = null;
            try
            {
                data["b"] = 2;
            }
            catch (InvalidOperationException ex)
            {
                caught = ex;
            }
            Assert.AreEqual(1, count);
            Assert.IsNotNull(caught);
        }

        [TestMethod()]
        public void StructuredStringMessageAndProperty()
        {
            var properties = new Dictionary<string, object>() {
                { "a", 1 },
            };

            IStructuredData data = new StructuredData(properties, "X");
            var actual = data.ToString();

            Assert.AreEqual("X; a=1", actual);
        }

        [TestMethod()]
        public void StructuredStringPropetiesAndTemplateValues()
        {
            var properties = new Dictionary<string, object>() {
                { "a", 1 },
                { "b", 2 },
                { "d", 4 },
            };

            IStructuredData data = new StructuredData(properties, "x{b}y{c}z{d}", "B", "C");
            var actual = data.ToString();

            Assert.AreEqual(@"xByCz4; a=1", actual);
        }

        [TestMethod()]
        public void StructuredStringExtraValues()
        {
            IStructuredData data = new StructuredData("x{a}", 1, 2);
            var actual = data.ToString();

            Assert.AreEqual(@"x1; Extra1=2", actual);
        }

        [TestMethod()]
        public void StructuredStringWithException()
        {
            var template = "x{a}";

            IStructuredData data;
            try
            {
                throw new ApplicationException("B");
            }
            catch (Exception ex)
            {
                data = new StructuredData(ex, template, "A");
            }
            var actual = data.ToString();

            var expectedMessageWithStartOfStackTrace = "xA; Exception='System.ApplicationException: B\r\n   at Essential.Diagnostics.Tests.StructuredDataTests.StructuredStringWithException() ";
            StringAssert.StartsWith(actual, expectedMessageWithStartOfStackTrace);
        }

        [TestMethod()]
        public void StructuredStringWithAdditionalData()
        {
            var template = "x{a}";
            var additional = new Dictionary<string, object>() { { "b", "B" } };

            IStructuredData data = new StructuredData(additional, template, "A");
            var actual = data.ToString();

            Assert.AreEqual("xA; b='B'", actual);
        }

        [TestMethod()]
        public void StructuredStringWithDuplicateTemplateValues()
        {
            IStructuredData data = new StructuredData("{a}x{a}", "A", "B");
            var actual = data.ToString();

            Assert.AreEqual("BxB", actual);
        }
    }
}
