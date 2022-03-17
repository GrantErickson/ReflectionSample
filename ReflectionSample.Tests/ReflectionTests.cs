using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReflectionSample.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public void ReflectOnStringBuilder()
        {
            var t = typeof(StringBuilder);
            var sb = new StringBuilder();
            var tsb = sb.GetType();
            Assert.AreEqual(t.Name, tsb.Name);
            Assert.AreSame(t, tsb);
            Assert.AreEqual(0, t.GetFields().Length);
            Assert.AreEqual(6, t.GetConstructors().Length);
            Assert.AreEqual(1, t.GetConstructors().Skip(1).First().GetParameters().Length);
            Assert.AreEqual(1, t.GetInterfaces().Length);
            Assert.AreEqual("ISerializable", t.GetInterfaces().First().Name);
            Assert.AreEqual(99, t.GetMembers().Length);
            Assert.AreEqual(4, t.GetProperties().Length);
            Assert.AreEqual(typeof(object), t.BaseType);
            Assert.AreEqual(null, t.BaseType!.BaseType);
            Assert.AreEqual(5, t.GetCustomAttributes(true).Length);
        }


        [TestMethod]
        public void CreateStringBuilder()
        {
            var t = typeof(StringBuilder);

            dynamic sb = System.Activator.CreateInstance(t)!;
            Assert.AreEqual(16, sb.Capacity);
            sb!.Append("Test");
            sb!.Append("123");
            sb!.AppendLine("123");
            Assert.AreEqual("Test123123" + Environment.NewLine, sb.ToString());
        }

        [TestMethod]
        public void CreateStringBuilderWithParams()
        {
            var t = typeof(StringBuilder);

            object[] parameters = { 500 };
            dynamic sb = Activator.CreateInstance(t, parameters)!;
            Assert.AreEqual(500, sb.Capacity);
            Assert.AreEqual(500, t.GetProperties().First(f => f.Name == "Capacity").GetValue(sb));
            sb.Append("Test");
            Assert.AreEqual(4, sb.Length);
            t.GetMethod("Clear")!.Invoke(sb, null);
            Assert.AreEqual(0, sb.Length);
            var appendString = t.GetMethods().First(f=>f.Name == "Append" && f.GetParameters().Length == 1 && f.GetParameters().First().ParameterType == typeof(string));
            appendString.Invoke(sb, new object[] { "Test" });
            Assert.AreEqual(4, sb.Length);
            Assert.ThrowsException<ArgumentException>(() =>
            {
                appendString.Invoke(sb, new object[] { 4 });
            });
            var appendInt = t.GetMethods().First(f => f.Name == "Append" && f.GetParameters().Length == 1 && f.GetParameters().First().ParameterType == typeof(int));
            appendInt.Invoke(sb, new object[] { 4 }); 
            Assert.AreEqual("Test4", sb.ToString());
        }

        [TestMethod]
        public void AdderTest()
        {
            IAdder adder = (IAdder)Activator.CreateInstance("ReflectionSample", "ReflectionSample.SpecialClass")!.Unwrap()!;
            Assert.AreEqual(5, adder.Add(1, 4));
        }


        [TestMethod]
        public void AdderTestPrivate()
        {
            IAdder adder = (IAdder)Activator.CreateInstance("ReflectionSample", "ReflectionSample.PrivateAdder")!.Unwrap()!;
            Assert.AreEqual(5, adder.Add(1, 4));
            Assert.ThrowsException<RuntimeBinderException>(() =>
            {
                Assert.AreEqual(5, ((dynamic)adder).lastResult);
            });
            Assert.AreEqual(5, adder.GetType().GetField("lastResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(adder));
        }

        [TestMethod]
        public void GetAssemblyClasses()
        {
            Assembly rs = Assembly.Load("ReflectionSample");
            Assert.AreEqual("ReflectionSample", rs.GetName().Name);
            Assert.IsTrue(rs.GetTypes().Any(f => f.Name == "SpecialClass"));
            Assert.IsTrue(rs.GetTypes().Any(f => f.Name == "IAdder"));
            Assert.IsTrue(rs.GetTypes().Any(f => f.Name == "PrivateAdder"));
            //Assert.AreEqual("<PrivateImplementationDetails>", rs.GetTypes().Skip(3).First().Name);
            Assert.AreEqual(2, rs.GetTypes().Count(f=>f.GetInterfaces().Contains(typeof(IAdder))));
        }

    }
}