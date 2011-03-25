using System;
using System.Collections.Generic;
using System.Linq;

namespace NorthHorizon.Common.Xmp
{
	/// <summary>
	/// Provides extensions methods for <see cref="Type"/>.
	/// </summary>
	public static class TypeExtensions
	{
		private static readonly ICollection<Type> IntegerNumericTypes = new[] { typeof(byte), typeof(short), typeof(int), typeof(long) };
		private static readonly ICollection<Type> DecimalNumericTypes = new[] { typeof(float), typeof(double), typeof(decimal) };

#if !NET40
		public static bool GetIsInteger(this Type type)
		{
			return GetIsInteger(type, false);
		}

		public static bool GetIsInteger(this Type type, bool includeNullables)
#else
		public static bool GetIsInteger(this Type type, bool includeNullables = false)
#endif
		{
			if (type == null)
				throw new ArgumentNullException("type");

			var target = includeNullables ? StripNullable(type) : type;

			return IntegerNumericTypes.Contains(target);
		}

#if !NET40
		public static bool GetIsNumeric(this Type type)
		{
			return GetIsNumeric(type, false);
		}

		public static bool GetIsNumeric(this Type type, bool includeNullables)
#else
		public static bool GetIsNumeric(this Type type, bool includeNullables = false)
#endif
		{
			if (type == null)
				throw new ArgumentNullException("type");

			var target = includeNullables ? StripNullable(type) : type;

			return IntegerNumericTypes.Union(DecimalNumericTypes).Contains(target);
		}

		private static Type StripNullable(Type type)
		{
			return
				type.IsGenericType &&
				!type.IsGenericTypeDefinition &&
				type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ?
				type.GetGenericArguments()[0] :
				type;
		}

		/// <summary>
		/// Gets whether the target type closes a specified open generic type.
		/// </summary>
		/// <param name="target">The target type.</param>
		/// <param name="openType">The open generic type.</param>
		/// <returns>Whether <paramref name="target"/> closes <paramref name="openType"/>.</returns>
		public static bool ClosesOpenType(this Type target, Type openType)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (openType == null)
				throw new ArgumentNullException("openType");

			if (!openType.IsGenericTypeDefinition)
				throw new ArgumentException("must be an open generic type", "openType");

			if (target.IsGenericTypeDefinition)
				return false;

			if (openType.IsInterface)
			{
				foreach (var interfaceType in target.GetInterfaces())
					if (ClosesOpenTypeExact(interfaceType, openType))
						return true;

				return false;
			}

			var closedType = target;
			while (closedType != null)
			{
				if (ClosesOpenTypeExact(closedType, openType))
					return true;

				closedType = closedType.BaseType;
			}

			return false;
		}

		private static bool ClosesOpenTypeExact(Type target, Type openType)
		{
			return 
				target.IsGenericType && 
				!target.IsGenericTypeDefinition &&
				target.GetGenericTypeDefinition().Equals(openType);
		}
	}
}
