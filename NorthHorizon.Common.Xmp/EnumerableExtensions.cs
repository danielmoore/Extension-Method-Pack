using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

			foreach (var colItem in collection)
				yield return colItem;

			yield return item;
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

			yield return item;

			foreach (var colItem in collection)
				yield return colItem;
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

			return collection as ObservableCollection<T> ?? new ObservableCollection<T>(collection);
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

			return collection as HashSet<T> ?? new HashSet<T>(collection);
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
	}
}
