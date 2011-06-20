using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NorthHorizon.Common.Xmp
{
	/// <summary>
	/// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/>.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Adds a value to the hashtable.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="item">The item to add.</param>
		public static void Add<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
			where TCol : ICollection<TItem>, new()
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			TCol col;
			if (dictionary.TryGetValue(key, out col))
			{
				if (col.IsReadOnly)
					throw new InvalidOperationException("bucket is read only");
			}
			else
				dictionary.Add(key, col = new TCol());

			col.Add(item);
		}

		/// <summary>
		/// Adds a value to the hashtable.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="additions">The items to add.</param>
		public static void AddAll<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, IEnumerable<TItem> additions)
			where TCol : ICollection<TItem>, new()
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			if (additions == null)
				throw new ArgumentNullException("additions");

			TCol col;
			if (!dictionary.TryGetValue(key, out col))
				dictionary.Add(key, col = new TCol());

			foreach (var item in additions)
				col.Add(item);
		}

		/// <summary>
		/// Removes a value from the hashtable.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="item">The item to remove.</param>
		public static void Remove<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			TCol col;
			if (dictionary.TryGetValue(key, out col))
				col.Remove(item);
		}

		/// <summary>
		/// Removes a value from the specified key's bucket and removes the
		/// key as well if the resulting bucket is empty.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="item">The item to remove.</param>
		public static void RemoveAndClean<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			TCol col;
			if (dictionary.TryGetValue(key, out col))
			{
				col.Remove(item);

				if (col.Count == 0)
					dictionary.Remove(key);
			}
		}

		/// <summary>
		/// Removes the specified values from the specified key's bucket.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="removals">The items to remove.</param>
		public static void RemoveAll<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, IEnumerable<TItem> removals)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			if (removals == null)
				throw new ArgumentNullException("removals");

			TCol col;
			if (dictionary.TryGetValue(key, out col))
				foreach (var item in removals)
					col.Remove(item);
		}

		/// <summary>
		/// Removes the specified values from the specified key's bucket and
		/// removes the key as well if the resulting bucket is empty.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="removals">The items to remove.</param>
		public static void RemoveAllAndClean<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, IEnumerable<TItem> removals)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			if (removals == null)
				throw new ArgumentNullException("removals");

			TCol col;
			if (dictionary.TryGetValue(key, out col))
			{
				foreach (var item in removals)
					col.Remove(item);

				if (col.Count == 0)
					dictionary.Remove(key);
			}
		}

		/// <summary>
		/// Removes all values from the specified key's bucket
		/// determined by <paramref name="predicate"/> and returns them.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="predicate">The predicate determining which items to remove.</param>
		/// <returns>The removed items.</returns>
		public static IEnumerable<TItem> RemoveAll<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, Predicate<TItem> predicate)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			if (predicate == null)
				throw new ArgumentNullException("predicate");

			var removals = new List<TItem>();

			TCol col;
			if (dictionary.TryGetValue(key, out col))
			{
				foreach (var item in col)
					if (predicate(item))
						removals.Add(item);

				foreach (var item in removals)
					col.Remove(item);
			}

			return removals;
		}

		/// <summary>
		/// Removes the specified values from the hashtable, removes the
		/// key if the resulting bucket is empty, and returns the removed items.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <typeparam name="TItem">The type of the item values.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		/// <param name="key">The target key.</param>
		/// <param name="predicate">The predicate determining which items to remove.</param>
		/// <returns>The removed items.</returns>
		public static IEnumerable<TItem> RemoveAllAndClean<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, Predicate<TItem> predicate)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			if (key == null)
				throw new ArgumentNullException("key");

			if (predicate == null)
				throw new ArgumentNullException("predicate");

			var removals = new List<TItem>();

			TCol col;
			if (dictionary.TryGetValue(key, out col))
			{
				foreach (var item in col)
					if (predicate(item))
						removals.Add(item);

				foreach (var item in removals)
					col.Remove(item);

				if (col.Count == 0)
					dictionary.Remove(key);
			}

			return removals;
		}

		/// <summary>
		/// Removes all keys from the hashtable with empty buckets.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TCol">The type of the value collection.</typeparam>
		/// <param name="dictionary">The target dictionary.</param>
		public static void Clean<TKey, TCol>(this IDictionary<TKey, TCol> dictionary)
			where TCol : ICollection
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			var keys = dictionary.Keys.ToList();

			foreach (var key in keys)
				if (dictionary[key].Count == 0)
					dictionary.Remove(key);
		}

		public static bool Contains<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
			where TCol : ICollection<TItem>
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			TCol col;
			return dictionary.TryGetValue(key, out col) && col.Contains(item);
		}

		public static bool DeepEquals<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> other)
		{
			return target.DeepEquals(other, EqualityComparer<TValue>.Default.Equals);
		}

		public static bool DeepEquals<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> other, Func<TValue, TValue, bool> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");

			if (target == null && other == null)
				return true;

			if (target == null || other == null)
				return false;

			if (object.ReferenceEquals(target, other))
				return true;

			if (target.Count != other.Count)
				return false;

			foreach (var pair in target)
			{
				TValue otherValue;
				if (!other.TryGetValue(pair.Key, out otherValue) || !comparer(pair.Value, otherValue))
					return false;
			}

			return true;
		}
	}
}
