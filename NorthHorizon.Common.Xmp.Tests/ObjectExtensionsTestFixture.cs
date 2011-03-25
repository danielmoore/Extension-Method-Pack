using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class ObjectExtensionsTestFixture
	{
		[TestMethod]
		public void TestAsForProperType()
		{
			var test = new TestClass();
			object obj = test;
			var success = obj.As<TestClass>(t => t.Call());
			Assert.IsTrue(success);
			Assert.AreEqual(1, test.Calls);
		}

		[TestMethod]
		public void TestAsForDifferentType()
		{
			var test = new TestClass();
			object obj = test;
			var success = obj.As<HashSet<string>>(s => s.Clear());
			Assert.IsFalse(success);
			Assert.AreEqual(0, test.Calls);
		}

		[TestMethod]
		public void TestAsValueTypeForProperType()
		{
			int test = 1;
			object obj = test;
			var success = obj.AsValueType<int>(t => test = ++t);
			Assert.IsTrue(success);
			Assert.AreEqual(2, test);
		}

		[TestMethod]
		public void TestAsValueTypeForDifferentType()
		{
			double test = 1;
			object obj = test;
			var success = obj.AsValueType<int>(t => test = ++t);
			Assert.IsFalse(success);
			Assert.AreEqual(1, test);
		}


		[TestMethod]
		public void TestChainGetSuccess()
		{
			var chain = new TestNode(0, new TestNode(1, new TestNode(2, new TestNode(3, null))));

			bool success;
			var id = chain.ChainGet(c => c.Node.Node.Node.Id, out success);
			Assert.IsTrue(success);
			Assert.AreEqual(3, id);
		}

		[TestMethod]
		public void TestChainGetFail()
		{
			var chain = new TestNode(0, new TestNode(1, new TestNode(2, new TestNode(3, null))));

			bool success;
			var id = chain.ChainGet(c => c.Node.Node.Node.Node.Id, out success);
			Assert.IsFalse(success);
		}

		[TestMethod]
		public void TestChainGetAccessTime()
		{
			const int repeat = 10000;

			var chain = new TestNode(0, new TestNode(1, new TestNode(2, new TestNode(3, null))));

			int id = 0;

			var normalStopwatch = Stopwatch.StartNew();
			for (int i = 0; i < repeat; i++)
				if (chain != null)
					if (chain.Node != null)
						if (chain.Node.Node != null)
							if (chain.Node.Node.Node != null)
								id = chain.Node.Node.Node.Id;
			normalStopwatch.Stop();

			var chainStopwatch = Stopwatch.StartNew();
			for (int i = 0; i < repeat; i++)
				id = chain.ChainGet(c => c.Node.Node.Node.Id);
			chainStopwatch.Stop();

			Assert.Inconclusive("Normal conditional null checking took {0} ticks, ChainGet took {1} ticks.", normalStopwatch.ElapsedTicks, chainStopwatch.ElapsedTicks);
		}

		[TestMethod, Ignore]
		public void TestChainGetWithIndexes()
		{
			var chain = new TestTree(0) { Nodes = new[] { new TestNode(1, new TestNode(2, null)) } };

			var id = chain.ChainGet(c => c.Nodes[0].Node.Node.Id);

			Assert.AreEqual(2, id);
		}

		private class TestClass
		{
			public int Calls { get; private set; }

			public void Call()
			{
				Calls++;
			}
		}

		private class TestNode
		{
			public TestNode(int id, TestNode node)
			{
				Id = id;
				Node = node;
			}

			public int Id { get; private set; }
			public TestNode Node { get; private set; }
		}

		private class TestTree : TestNode
		{
			public TestTree(int id)
				: base(id, null)
			{
			}

			public TestNode this[int index]
			{
				get { return Nodes[index]; }
			}

			public TestNode[] Nodes { get; set; }
		}
	}
}
