using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class EnumerableExtensionsTestFixture
	{
		[TestMethod]
		public void TestGenericForEach()
		{
			var list = new List<TestClass>();
			for (int i = 0; i < 5; i++)
				list.Add(new TestClass());

			list.ForEach(t => t.Call());

			foreach (var item in list)
				Assert.AreEqual(1, item.Calls);
		}

		[TestMethod]
		public void TestWeaklyTypedForEach()
		{
			var list = new ArrayList();
			for (int i = 0; i < 5; i++)
				list.Add(new TestClass());

			list.ForEach(t => ((TestClass)t).Call());

			foreach (var item in list.Cast<TestClass>())
				Assert.AreEqual(1, item.Calls);
		}

		[TestMethod]
		public void TestAppend()
		{
			var list = new List<int> { 1, 2 };
			var newList = list
				.Append(3)
				.Append(4)
				.Append(5);

			int i = 1;
			foreach (var item in newList)
				Assert.AreEqual(i++, item);

			Assert.AreEqual(6, i);
		}

		[TestMethod]
		public void TestPrepend()
		{
			var list = new List<int> { 4, 5 };
			var newList = list
				.Prepend(3)
				.Prepend(2)
				.Prepend(1);

			int i = 1;
			foreach (var item in newList)
				Assert.AreEqual(i++, item);

			Assert.AreEqual(6, i);
		}

		[TestMethod]
		public void TestAsObservableCreation()
		{
			IEnumerable<int> list = new List<int> { 1, 2, 3, 4, 5 };
			var obs = list.AsObservable();

			Assert.AreNotSame(list, obs);

			int i = 1;
			foreach (var item in obs)
				Assert.AreEqual(i++, item);

			Assert.AreEqual(6, i);
		}

		[TestMethod]
		public void TestAsObservableReuse()
		{
			IEnumerable<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
			var obs = list.AsObservable();

			Assert.AreSame(list, obs);
		}

		[TestMethod]
		public void TestAsHashSetCreation()
		{
			IEnumerable<int> list = new List<int> { 0, 1, 1, 2, 3, 4, 4 };
			var set = list.AsHashSet();

			Assert.AreNotSame(list, set);

			Assert.AreEqual(5, set.Count);

			var array = new int[set.Count];
			set.CopyTo(array);
			Array.Sort(array);

			for (int i = 0; i < array.Length; i++)
				Assert.AreEqual(i, array[i]);
		}

		[TestMethod]
		public void TestAsHashSetReuse()
		{
			IEnumerable<int> list = new HashSet<int> { 1, 2, 3, 4, 5 };
			var set = list.AsHashSet();

			Assert.AreSame(list, set);
		}

		[TestMethod]
		public void TestArgMax()
		{
			var list = new List<double> { -5, -3, -1, 1, 3 };
			var argmax = list.ArgMax(Math.Abs);
			Assert.AreEqual(-5, argmax);
		}

		[TestMethod]
		public void TestArgMin()
		{
			var list = new List<double> { -5, -3, -2 , 1 };
			var argmax = list.ArgMin(Math.Abs);
			Assert.AreEqual(1, argmax);
		}

		private class TestClass
		{
			public int Calls { get; private set; }

			public void Call()
			{
				Calls++;
			}
		}

	}
}
