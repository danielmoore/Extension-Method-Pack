using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class TypeExtensionsTestFixture
	{
		[TestMethod]
		public void TestBaseClassImplementation()
		{
			var result = typeof(TestClass).ClosesOpenType(typeof(List<>));

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void TestBaseInterfaceImplementation()
		{
			var result = typeof(TestClass).ClosesOpenType(typeof(IEnumerable<>));

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void TestComplexBasClass()
		{
			var result = typeof(ComplexTestClass).ClosesOpenType(typeof(ComplexTestClass<,,>));

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void TestComplexInterface()
		{
			var result = typeof(ComplexTestClass).ClosesOpenType(typeof(IComplexTestInterface1<,,>));

			Assert.IsTrue(result);
		}

		private class TestClass : List<int> { }

		private class ComplexTestClass<T1, T2, T3> : IComplexTestInterface1<T1, T2, T3>, IComplexTestInterface2<T1, T2, T3> {}

		private class ComplexTestClass<T1, T2> : ComplexTestClass<T1, T2, int>, IComplexTestInterface1<T1, T2>, IComplexTestInterface2<T1, T2> { }

		private class ComplexTestClass<T1> : ComplexTestClass<T1, int>, IComplexTestInterface1<T1>, IComplexTestInterface2<T1> { }

		private class ComplexTestClass : ComplexTestClass<int>, IComplexTestInterface1, IComplexTestInterface2 { }

		private interface IComplexTestInterface1<T1, T2, T3> { }

		private interface IComplexTestInterface1<T1, T2> : IComplexTestInterface1<T1, T2, int> { }

		private interface IComplexTestInterface1<T1> : IComplexTestInterface1<T1, int> { }

		private interface IComplexTestInterface1 : IComplexTestInterface1<int> { }

		private interface IComplexTestInterface2<T1, T2, T3> { }

		private interface IComplexTestInterface2<T1, T2> : IComplexTestInterface2<T1, T2, int> { }

		private interface IComplexTestInterface2<T1> : IComplexTestInterface2<T1, int> { }

		private interface IComplexTestInterface2 : IComplexTestInterface2<int> { }
	}
}
