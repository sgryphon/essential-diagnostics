using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics.Tests
{
    [TestClass()]
    public class StucturedDataFormatterTests
    {
        [TestMethod()]
        public void StructuredEscapePropertyName()
        {
            var properties = new Dictionary<string, object>() {
                { "a b=c", 1 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a_bc=1", actual);
        }

        [TestMethod()]
        public void StructuredBasicStringValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", "A" },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a='A'", actual);
        }

        [TestMethod()]
        public void StructuredDateTimeValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new DateTime(2001, 2, 3, 4, 5, 6, 7, DateTimeKind.Utc) },
                { "b", new DateTime(2002, 2, 3, 4, 5, 6, 7, DateTimeKind.Local) },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=2001-02-03T04:05:06 b=2002-02-03T04:05:06", actual);
        }

        [TestMethod()]
        public void StructuredDateTimeOffsetValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new DateTimeOffset(2001, 2, 3, 4, 5, 6, 7, TimeSpan.Zero) },
                { "b", new DateTimeOffset(2002, 2, 3, 4, 5, 6, 7, TimeSpan.FromHours(10)) },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=2001-02-03T04:05:06+00:00 b=2002-02-03T04:05:06+10:00", actual);
        }

        [TestMethod()]
        public void StructuredByteArray()
        {
            var byteArray1 = new byte[] { 0xa1, 0, 5, 255 };
            var byteArray2 = new byte[0];
            var byteArray3 = new List<byte>() { 1, 2 };
            var properties = new Dictionary<string, object>() {
                { "a", byteArray1 },
                { "b", byteArray2 },
                { "c", byteArray3 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=A10005FF b= c=[0x01,0x02]", actual);
        }

        [TestMethod()]
        public void StructuredByteValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", (byte)0 },
                { "b", (byte)0xa1 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=0x00 b=0xA1", actual);
        }

        [TestMethod()]
        public void StructuredTimeSpanValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new TimeSpan(0, 0, 0, 4, 5) },
                { "b", TimeSpan.Zero - new TimeSpan(1, 2, 3, 4, 5) },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=00:00:04.0050000 b=-1.02:03:04.0050000", actual);
        }

        [TestMethod()]
        public void StructuredPrimitiveValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", (short)-1 },
                { "b", (int)-2 },
                { "c", (long)-3 },
                { "d", (ushort)4 },
                { "e", (uint)5 },
                { "f", (ulong)6 },
                { "g", (sbyte)7 },
                { "h", (float)8.1 },
                { "i", (double)9.2 },
                { "j", (decimal)10.3 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=-1 b=-2 c=-3 d=4 e=5 f=6 g=7 h=8.1 i=9.2 j=10.3", actual);
        }

        [TestMethod()]
        public void StructuredNullableValue()
        {
            int? b = -2;
            int? k = null;
            var properties = new Dictionary<string, object>() {
                { "a", (short?)-1 },
                { "b", b },
                { "c", (long?)-3 },
                { "d", (ushort?)4 },
                { "e", (uint?)5 },
                { "f", (ulong?)6 },
                { "g", (sbyte?)7 },
                { "h", (float?)8.1 },
                { "i", (double?)9.2 },
                { "j", (decimal?)10.3 },
                { "k", k },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Console.WriteLine(actual);
            Assert.AreEqual("a=-1 b=-2 c=-3 d=4 e=5 f=6 g=7 h=8.1 i=9.2 j=10.3 k=null", actual);
        }

        [TestMethod()]
        public void StructuredNullableOtherValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", (bool?)true },
                { "b", (bool?)null },
                { "c", (DateTime?)new DateTime(2001, 2, 3, 4, 5, 6, 8, DateTimeKind.Unspecified) },
                { "d", (DateTimeOffset?)new DateTimeOffset(2002, 3, 4, 5, 6, 7, 8, TimeSpan.FromHours(10)) },
                { "e", (TimeSpan?)new TimeSpan(1, 2, 3, 4, 5) },
                { "f", (Guid?)new Guid("12345678-abcd-4321-8765-ba9876543210") },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=true b=null c=2001-02-03T04:05:06 d=2002-03-04T05:06:07+10:00 e=1.02:03:04.0050000 f=12345678-abcd-4321-8765-ba9876543210", actual);
        }

        [TestMethod()]
        public void StructuredGuidValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new Guid("12345678-abcd-4321-8765-ba9876543210") },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=12345678-abcd-4321-8765-ba9876543210", actual);
        }

        [TestMethod()]
        public void StructuredArrayValues()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new int[] { 1, 2, 3 } },
                { "b", new List<string>() { "A", "B", "C" } },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=[1,2,3] b=['A','B','C']", actual);
        }

        [TestMethod()]
        public void StructuredChildArray()
        {
            var child1 = new ArrayList() { 1, "A" };

            var list1 = new ArrayList() { "x", child1 };
            var obj2 = new TestObject2() { M = "y", N = child1 };
            var dict3 = new Dictionary<string, object>()
            {
                { "P", "z" },
                { "Q", child1 }
            };

            var properties = new Dictionary<string, object>() {
                { "a", list1 },
                { "@b", obj2 },
                { "c", dict3 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=['x','System.Collections.ArrayList'] b=(M='y' N=[1,'A']) c=(P='z' Q=[1,'A'])", actual);
        }

        [TestMethod()]
        public void StructuredChildDictionary()
        {
            var child1 = new Dictionary<string, object>()
            {
                { "X", "A" },
                { "Y", 1 }
            };

            var list1 = new ArrayList() { "x", child1 };
            var obj2 = new TestObject2() { M = "y", N = child1 };

            var properties = new Dictionary<string, object>() {
                { "a", list1 },
                { "@b", obj2 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=['x','System.Collections.Generic.Dictionary`2[System.String,System.Object]'] b=(M='y' N='System.Collections.Generic.Dictionary`2[System.String,System.Object]')", actual);
        }

        [TestMethod()]
        public void StructuredRecursiveListShouldStop()
        {
            var child1 = new ArrayList() { 1, "A" };
            var list1 = new ArrayList() { 2, child1 };
            child1.Add(list1);

            var properties = new Dictionary<string, object>() {
                { "a", list1 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=[2,'System.Collections.ArrayList']", actual);
        }


        [TestMethod()]
        public void StructuredRecursiveDictionaryShouldStop()
        {
            var child1 = new Dictionary<string, object>()
            {
                { "Y", 1 }
            };
            var dictionary1 = new Dictionary<string, object>()
            {
                { "X", 2 },
                { "Z", child1 }
            };
            child1.Add("Q", dictionary1);

            var properties = new Dictionary<string, object>() {
                { "a", dictionary1 },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual("a=(X=2 Z='System.Collections.Generic.Dictionary`2[System.String,System.Object]')", actual);
        }

        [TestMethod()]
        public void StructuredEscapedStringValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", @"w=x\y'z" },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual(@"a='w=x\\y\'z'", actual);
        }

        [TestMethod()]
        public void StructuredCustomObjectValue()
        {
            var properties = new Dictionary<string, object>() {
                { "a", new TestObject() },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual(@"a='w=x\\y\'z'", actual);
        }

        [TestMethod()]
        public void DestructuredCustomObjectMessage()
        {
            var testObject = new TestObject() { X = 1.2, Y = 3.4 };

            IStructuredData data = new StructuredData("z{@a}", testObject);
            var actual = data.ToString();

            Assert.AreEqual(@"z(X=1.2 Y=3.4)", actual);
        }

        [TestMethod()]
        public void DestructuredCustomObjectProperty()
        {
            var testObject = new TestObject() { X = 1.2, Y = 3.4 };
            var properties = new Dictionary<string, object>() {
                { "@a", testObject },
            };

            IStructuredData data = new StructuredData(properties);
            var actual = data.ToString();

            Assert.AreEqual(@"a=(X=1.2 Y=3.4)", actual);
        }

        class TestObject
        {
            public double X { get; set; }

            public double Y { get; set; }

            public override string ToString()
            {
                return @"w=x\y'z";
            }
        }

        class TestObject2
        {
            public object M { get; set; }

            public object N { get; set; }
        }

    }
}
