using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NorthHorizon.Common.Xmp
{
	/// <summary>
	/// Provides extension methods for all objects.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Executes the specified action if <paramref name="obj"/> is of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The desired type.</typeparam>
		/// <param name="obj">The target object.</param>
		/// <param name="action">The action to perform.</param>
		/// <returns>Whether or not the action was able to be performed.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static bool As<T>(this object obj, Action<T> action) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (action == null)
				throw new ArgumentNullException("action");

			var target = obj as T;
			if (target == null)
				return false;

			action(target);
			return true;
		}

		/// <summary>
		/// Executes the specified action if <paramref name="obj"/> is of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The desired type.</typeparam>
		/// <param name="obj">The target object.</param>
		/// <param name="action">The action to perform.</param>
		/// <returns>Whether or not the action was able to be performed.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
		public static bool AsValueType<T>(this object obj, Action<T> action) where T : struct
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (action == null)
				throw new ArgumentNullException("action");

			if (obj is T)
			{
				action((T)obj);
				return true;
			}
			
			return false;
		}

		/// <summary>
		/// Gets whether the target object is in the list, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the target object.</typeparam>
		/// <param name="item">The target object.</param>
		/// <param name="list">The list of items to compare against.</param>
		/// <returns>Whether <paramref name="item"/> is in <paramref name="list"/></returns>
		public static bool In<T>(this T item, params T[] list)
		{
			return item.In(EqualityComparer<T>.Default.Equals, list);
		}

		/// <summary>
		/// Gets whether the target object is in the list, using a default comparer.
		/// </summary>
		/// <typeparam name="T">The type of the target object.</typeparam>
		/// <param name="item">The target object.</param>
		/// <param name="comparer">A function that yields whether two objects are equal.</param>
		/// <param name="list">The list of items to compare against.</param>
		/// <returns>Whether <paramref name="item"/> is in <paramref name="list"/></returns>
		public static bool In<T>(this T item, Func<T, T, bool> comparer, params T[] list)
		{
			// NOTE: item can be null!

			if (comparer == null)
				throw new ArgumentNullException("comparer");

			if (list == null)
				throw new ArgumentNullException("list");

			foreach (var listItem in list)
				if (comparer(item, listItem))
					return true;

			return false;
		}

		/// <summary>
		/// Gets a specified property in a chain of member accesses, checking for null at each node.
		/// </summary>
		/// <typeparam name="TRoot">The type of the root object.</typeparam>
		/// <typeparam name="TValue">The type of the final node.</typeparam>
		/// <param name="root">The target object</param>
		/// <param name="getExpression">The expression representing the member access chain.</param>
		/// <returns>The value of the target member or <code>default(TValue)</code></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static TValue ChainGet<TRoot, TValue>(this TRoot root, Expression<Func<TRoot, TValue>> getExpression)
		{
			bool success;
			return ChainGet(root, getExpression, out success);
		}

		/// <summary>
		/// Gets a specified property in a chain of member accesses, checking for null at each node.
		/// </summary>
		/// <typeparam name="TRoot">The type of the root object.</typeparam>
		/// <typeparam name="TValue">The type of the final node.</typeparam>
		/// <param name="root">The target object</param>
		/// <param name="getExpression">The expression representing the member access chain.</param>
		/// <param name="success">Whether or not the chain was completely evaluated.</param>
		/// <returns>The value of the target member or <code>default(TValue)</code></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
		public static TValue ChainGet<TRoot, TValue>(this TRoot root, Expression<Func<TRoot, TValue>> getExpression, out bool success)
		{
			if (getExpression == null)
				throw new ArgumentNullException("getExpression");

			if (root == null)
				goto fail;

			var members = new Stack<MemberAccessInfo>();

			Expression expr = getExpression.Body;
			while (expr != null)
			{
				if (expr.NodeType == ExpressionType.Parameter)
					break;

				var memberExpr = expr as MemberExpression;
				if (memberExpr == null)
					throw new ArgumentException("Given expression is not a member access chain.", "getExpression");

				members.Push(new MemberAccessInfo(memberExpr.Member));

				expr = memberExpr.Expression;
			}

			object node = root;
			foreach (var member in members)
			{
				if (node == null)
					goto fail;

				node = member.GetValue(node);
			}

			success = true;
			return (TValue)node;

			fail:
			success = false;
			return default(TValue);
		}

		private class MemberAccessInfo
		{
			private PropertyInfo _propertyInfo;
			private FieldInfo _fieldInfo;

			public MemberAccessInfo(MemberInfo info)
			{
				_propertyInfo = info as PropertyInfo;
				_fieldInfo = info as FieldInfo;
			}

			public object GetValue(object target)
			{
				if (_propertyInfo != null)
					return _propertyInfo.GetValue(target, null);
				else if (_fieldInfo != null)
					return _fieldInfo.GetValue(target);
				else
					throw new InvalidOperationException();
			}
		}
	}
}
