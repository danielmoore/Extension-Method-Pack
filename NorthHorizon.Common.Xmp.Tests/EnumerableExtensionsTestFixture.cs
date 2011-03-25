using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

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

		[TestMethod, Ignore]
		public void StressTestAppend()
		{
			IEnumerable<int> enumerable = new int[0];
			var list = new List<int>();
			for (int i = 0; i < 1E4; i++)
			{
				enumerable = enumerable.Append(i);
				list.Add(i);
			}

			var listStopwatch = Stopwatch.StartNew();
			foreach(var i in list);
			listStopwatch.Stop();

			var enumerableStopwatch = Stopwatch.StartNew();
			foreach (var i in enumerable) ;
			enumerableStopwatch.Stop();

			Assert.Inconclusive("List iterated in {0} ticks, appended enumerable iterated in {1} ticks.", listStopwatch.ElapsedTicks, enumerableStopwatch.ElapsedTicks);
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
		public void TestSkipExceptions()
		{
			var list = new[] { -1, 0, 1 };
			var newList = list.Select(i => 1 / i).SkipExceptions<int, DivideByZeroException>().ToArray();

			Assert.AreEqual(2, newList.Length);
			Assert.AreEqual(-1, newList[0]);
			Assert.AreEqual(1, newList[1]);
		}

		[TestMethod, Ignore]
		public void StressTestSkipExceptions()
		{
			var list = new List<string>();
			for (int i = 0; i < 1E5; i++)
			{
					list.Add(i.ToString());
			}

			var newList = new List<int>();

			var tryParseStopwtch = Stopwatch.StartNew();
			foreach (var str in list)
			{
				int value;
				if (int.TryParse(str, out value))
					newList.Add(value);
			}
			tryParseStopwtch.Stop();

			newList.Clear();
			var tryCatchStopWatch = Stopwatch.StartNew();
			foreach(var str in list)
				try
				{
					newList.Add(int.Parse(str));
				}
				catch { }
			tryCatchStopWatch.Stop();

			var skipExceptionsStopwatch = Stopwatch.StartNew();
			newList = list.Select(str => int.Parse(str)).SkipExceptions().ToList();
			skipExceptionsStopwatch.Stop();

			Assert.Inconclusive("TryParse iteration took {0} ticks, Try-Catch iteration took {1} ticks, SkipExceptions iteration took {1} ticks.", 
				tryParseStopwtch.ElapsedTicks, tryCatchStopWatch.ElapsedTicks, skipExceptionsStopwatch.ElapsedTicks);
		}

		[TestMethod]
		public void TestTryParse()
		{
			var list = new[] { "0", "1", "Foobar!", "2" };
			int[] ints = list.TryParse<string, int>().ToArray();

			for (int i = 0; i < 3; i++)
				Assert.AreEqual(i, ints[i]);
		}

		[TestMethod]
		public void TestAsObservable()
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
		public void TestAsHashSet()
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
