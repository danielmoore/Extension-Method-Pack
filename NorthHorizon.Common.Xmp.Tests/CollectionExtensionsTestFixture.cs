using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class CollectionExtensionsTestFixture
	{
		[TestMethod]
		public void TestAddAll()
		{
			var list = new List<int> { 0, 1, 2 };
			list.AddAll(new[] { 3, 4, 5 });

			for (int i = 0; i < list.Count; i++)
				Assert.AreEqual(i, list[i]);
		}

		[TestMethod]
		public void TestRemoveAllByPredicate()
		{
			var list = new List<int> { 1, 2, 3, 4, 5, 6 };
			list.RemoveAll(i => i < 4);

			for (int i = 0; i < list.Count; i++)
				Assert.AreEqual(i + 4, list[i]);
		}

		[TestMethod]
		public void TestRemoveAllByList()
		{
			var list = new List<int> { 1, 2, 3, 4, 5, 6 };
			var removals = new[] { 2, 3, 4 };
			list.RemoveAll(removals);

			Assert.AreEqual(1, list[0]);
			Assert.AreEqual(5, list[1]);
			Assert.AreEqual(6, list[2]);
		}
	}
}
