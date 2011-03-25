using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NorthHorizon.Common.Xmp
{
	/// <summary>
	/// Provides extension methods for <see cref="IEnumerable{T}"/> and <see cref="IEnumerable"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Executes the specified action with each item as the parameter.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <param name="action">The action to perform.</param>
		[Obsolete("Should use Run from RX.")]
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (action == null)
				throw new ArgumentNullException("action");

			foreach (var item in collection)
				action(item);
		}

		/// <summary>
		/// Executes the specified action with each item as the parameter.
		/// </summary>
		/// <param name="collection">The target collection</param>
		/// <param name="action">The action to perform.</param>
		[Obsolete("Should use Run from RX.")]
		public static void ForEach(this IEnumerable collection, Action<object> action)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (action == null)
				throw new ArgumentNullException("action");

			foreach (var item in collection)
				action(item);
		}

		/// <summary>
		/// Yields a new list with an item added to the end.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <param name="item">The item to append.</param>
		/// <returns>A new list with <paramref name="item"/> added to the end.</returns>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T item)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (item == null)
				throw new ArgumentNullException("item");

			var appendable = collection as AppendableEnumerable<T> ?? new AppendableEnumerable<T>(collection);
			return appendable.Append(item);
		}

		/// <summary>
		/// Yields a new list with an item added to the beginning.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <param name="item">The item to prepend.</param>
		/// <returns>A new list with <paramref name="item"/> added to the beginning.</returns>
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> collection, T item)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (item == null)
				throw new ArgumentNullException("item");

			var appendable = collection as AppendableEnumerable<T> ?? new AppendableEnumerable<T>(collection);
			return appendable.Prepend(item);
		}

		/// <summary>
		/// Yields a list without any items that throw an exception while enumerating.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>The list without any items that have thrown an exception.</returns>
		[Obsolete]
		public static IEnumerable<T> SkipExceptions<T>(this IEnumerable<T> collection)
		{
			return collection.SkipExceptions<T, Exception>();
		}

		/// <summary>
		/// Yields a list without any items that throw an exception while enumerating.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <typeparam name="TException">The type of exception to catch.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>The list without any items that have thrown an exception.</returns>
		[Obsolete]
		public static IEnumerable<T> SkipExceptions<T, TException>(this IEnumerable<T> collection) where TException : Exception
		{
			using (var enumerator = collection.GetEnumerator())
			{
				while (true)
				{
					bool success = false;
					bool exceptionOccurred = false;

					try
					{
						success = enumerator.MoveNext();
					}
					catch (TException)
					{
						exceptionOccurred = true;
					}

					// if an exception occurred, we don't know if we're at the end
					// of the list or not. Fortunately, calling MoveNext() after a
					// a list has been fully enumerated always returns false.
					if (!exceptionOccurred && !success)
						break;

					if (!exceptionOccurred)
						yield return enumerator.Current;
				}
			}
		}

		public static IEnumerable<TOutput> TryParse<TInput, TOutput>(this IEnumerable<TInput> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			var methodInfo = typeof(TOutput).GetMethod("TryParse", new[] { typeof(TInput), typeof(TOutput).MakeByRefType() });

			foreach (var item in collection)
			{
				var args = new object[] { item, null };
				var success = (bool)methodInfo.Invoke(null, args);

				if (success)
					yield return (TOutput)args[1];
			}
		}

		public static IEnumerable<TOutput> TryParse<TInput, TOutput>(this IEnumerable<TInput> collection, Func<TInput, TryParseResult<TOutput>, bool> tryParser)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (tryParser == null)
				throw new ArgumentNullException("tryParser");

			foreach (var item in collection)
			{
				var result = new TryParseResult<TOutput>();

				var success = tryParser(item, result);

				if (success)
					yield return result.Result;
			}
		}

		/// <summary>
		/// Returns the specified collection as an <see cref="ObservableCollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>
		/// An <see cref="ObservableCollection{T}"/> with all of the items
		/// in <paramref name="collection"/>. This is not necessarily a new object.
		/// </returns>
		public static ObservableCollection<T> AsObservable<T>(this IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			return new ObservableCollection<T>(collection);
		}

		/// <summary>
		/// Returns the specified collection as an <see cref="HashSet{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>
		/// An <see cref="HashSet{T}"/> with all of the items
		/// in <paramref name="collection"/>. This is not necessarily a new object.
		/// </returns>
		public static HashSet<T> AsHashSet<T>(this IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			return new HashSet<T>(collection);
		}

		/// <summary>
		/// Gets the argument that produces the maximum value 
		/// yielded from the specified function.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <typeparam name="TValue">The type of the value yielded from the specified function.</typeparam>
		/// <param name="collection">The target collection.</param>
		/// <param name="function">The function used to produce values.</param>
		/// <returns>The argument that produces the highest value.</returns>
		public static T ArgMax<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> function)
			where TValue : IComparable<TValue>
		{
			return ArgComp(collection, function, GreaterThan);
		}

		private static bool GreaterThan<T>(T first, T second) where T : IComparable<T>
		{
			return first.CompareTo(second) > 0;
		}

		/// <summary>
		/// Gets the argument that produces the minimum value 
		/// yielded from the specified function.
		/// </summary>
		/// <typeparam name="T">The type of items in the collection.</typeparam>
		/// <typeparam name="TValue">The type of the value yielded from the specified function.</typeparam>
		/// <param name="collection">The target collection.</param>
		/// <param name="function">The function used to produce values.</param>
		/// <returns>The argument that produces the least value.</returns>
		public static T ArgMin<T, TValue>(this IEnumerable<T> collection, Func<T, TValue> function)
			where TValue : IComparable<TValue>
		{
			return ArgComp(collection, function, LessThan);
		}

		private static bool LessThan<T>(T first, T second) where T : IComparable<T>
		{
			return first.CompareTo(second) < 0;
		}

		private static T ArgComp<T, TValue>(IEnumerable<T> collection, Func<T, TValue> function, Func<TValue, TValue, bool> accept)
			where TValue : IComparable<TValue>
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (function == null)
				throw new ArgumentNullException("function");

			var isSet = false;
			var maxArg = default(T);
			var maxValue = default(TValue);

			foreach (var item in collection)
			{
				var value = function(item);
				if (!isSet || accept(value, maxValue))
				{
					maxArg = item;
					maxValue = value;
					isSet = true;
				}
			}

			return maxArg;
		}

		/// <summary>
		/// Joins each item 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string StringJoin<T>(this IEnumerable<T> list, string separator)
		{
			return StringJoin(list, separator, ConvertObjectToString);
		}

		private static string ConvertObjectToString<T>(T obj)
		{
			return obj.ToString();
		}

		public static string StringJoin<T>(this IEnumerable<T> list, string separator, Func<T, string> toString)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			if (separator == null)
				throw new ArgumentNullException("separator");

			if (toString == null)
				throw new ArgumentNullException("toString");
#if NET40
			return string.Join(separator, list.Select(toString));
#else
			return string.Join(separator, list.Select(toString).ToArray());
#endif
		}

		public static bool DeepEquals<T>(this IEnumerable<T> target, IEnumerable<T> other)
		{
			return target.DeepEquals(other, EqualityComparer<T>.Default.Equals);
		}

		public static bool DeepEquals<T>(this IEnumerable<T> target, IEnumerable<T> other, Func<T, T, bool> comparer)
		{
			if (target == null && other == null)
				return true;

			if (target == null || other == null)
				return false;

			if (object.ReferenceEquals(target, other))
				return true;

			using (var targetEnumerable = target.GetEnumerator())
			using (var otherEnumerable = other.GetEnumerator())
			{
				while (true)
				{
					var targetSuccess = targetEnumerable.MoveNext();
					var otherSuccess = otherEnumerable.MoveNext();

					if (!targetSuccess && !otherSuccess)
						return true;

					if (!targetSuccess || !otherSuccess)
						return false;

					if (!comparer(targetEnumerable.Current, otherEnumerable.Current))
						return false;
				}
			}
		}

		private class AppendableEnumerable<T> : IEnumerable<T>
		{
			private readonly T[] _prepends;
			private readonly T[] _appends;

			private readonly IEnumerable<T> _baseEnumerable;

			public AppendableEnumerable(IEnumerable<T> baseEnumerable)
			{
				_prepends = new T[0];
				_appends = new T[0];

				_baseEnumerable = baseEnumerable;
			}

			private AppendableEnumerable(T[] prepends, IEnumerable<T> baseEnumerable, T[] appends)
			{
				_prepends = prepends;
				_baseEnumerable = baseEnumerable;
				_appends = appends;
			}

			public AppendableEnumerable<T> Prepend(T item)
			{
				var prepends = new T[_prepends.Length + 1];
				prepends[0] = item;
				_prepends.CopyTo(prepends, 1);

				return new AppendableEnumerable<T>(prepends, _baseEnumerable, _appends);
			}

			public AppendableEnumerable<T> Append(T item)
			{
				var appends = new T[_appends.Length + 1];
				_appends.CopyTo(appends, 0);
				appends[appends.Length - 1] = item;

				return new AppendableEnumerable<T>(_prepends, _baseEnumerable, appends);
			}

			public IEnumerator<T> GetEnumerator()
			{
				foreach (var item in _prepends)
					yield return item;

				foreach (var item in _baseEnumerable)
					yield return item;

				foreach (var item in _appends)
					yield return item;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public sealed class TryParseResult<T>
		{
			public T Result;
		}
	}
}
