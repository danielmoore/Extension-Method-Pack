using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class DictionaryExtensionsTestFixture
	{
		[TestMethod]
		public void TestAdd()
		{
			var table = new Dictionary<int, List<int>>()
			{
				{1,  new List<int>{1,2,3}},
				{2, new List<int>{2,4,6}}
			};

			table.Add(3, 3);
			table.Add(3, 6);
			table.Add(3, 9);

			int i = 1;
			foreach (var pair in table)
			{
				int j = 1;
				foreach (var item in pair.Value)
					Assert.AreEqual(i * j++, item);

				i++;
			}

			Assert.AreEqual(3, table.Count);
		}

		[TestMethod]
		public void TestAddAll()
		{
			var table = new Dictionary<int, List<int>>()
			{
				{1,  new List<int>{1,2,3}},
				{2, new List<int>{2,4,6}}
			};

			table.Add(3, new List<int> { 3, 6, 9 });

			int i = 1;
			foreach (var pair in table)
			{
				int j = 1;
				foreach (var item in pair.Value)
					Assert.AreEqual(i * j++, item);

				i++;
			}

			Assert.AreEqual(3, table.Count);
		}

		[TestMethod]
		public void TestRemove()
		{
			var table = new Dictionary<int, List<int>>()
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.Remove(3, 3);
			Assert.IsFalse(table[3].Contains(3));

			table.Remove(3, 6);
			Assert.IsFalse(table[3].Contains(6));

			table.Remove(3, 9);
			Assert.IsFalse(table[3].Contains(9));

			Assert.IsTrue(table.ContainsKey(3));
		}

		[TestMethod]
		public void TestRemoveAndClean()
		{
			var table = new Dictionary<int, List<int>>()
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.RemoveAndClean(3, 3);
			Assert.IsFalse(table[3].Contains(3));

			table.RemoveAndClean(3, 6);
			Assert.IsFalse(table[3].Contains(6));

			table.RemoveAndClean(3, 9);

			Assert.IsFalse(table.ContainsKey(3));
		}

		[TestMethod]
		public void TestRemoveAllByList()
		{
			var table = new Dictionary<int, List<int>>
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.RemoveAll(3, new[] { 3, 6, 9 });

			Assert.IsTrue(table.ContainsKey(3));
			Assert.AreEqual(0, table[3].Count);
		}

		[TestMethod]
		public void TestRemoveAllAndCleanByList()
		{
			var table = new Dictionary<int, List<int>>
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.RemoveAllAndClean(3, new[] { 3, 6, 9 });

			Assert.IsFalse(table.ContainsKey(3));
		}

		[TestMethod]
		public void TestRemoveAllByPredicate()
		{
			var table = new Dictionary<int, List<int>>
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.RemoveAll(3, new Predicate<int>(i => i > 0));

			Assert.IsTrue(table.ContainsKey(3));
			Assert.AreEqual(0, table[3].Count);
		}

		[TestMethod]
		public void TestRemoveAllAndCleanByPredicate()
		{
			var table = new Dictionary<int, List<int>>
			{
				{1, new List<int>{1, 2, 3}},
				{2, new List<int>{2, 4, 6}},
				{3, new List<int>{3, 6, 9}}
			};

			table.RemoveAllAndClean(3, new Predicate<int>(i => i > 0));

			Assert.IsFalse(table.ContainsKey(3));
		}

		[TestMethod]
		public void TestClean()
		{
			var table = new Dictionary<int, List<int>>
			{
				{1, new List<int>()},
				{2, new List<int>{2,4,6}},
				{3, new List<int>()}
			};

			table.Clean();

			Assert.IsFalse(table.ContainsKey(1));
			Assert.IsTrue(table.ContainsKey(2));
			Assert.IsFalse(table.ContainsKey(3));
		}
	}
}
