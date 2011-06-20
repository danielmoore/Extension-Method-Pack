using System;
using System.Windows;
using System.Windows.Threading;

namespace NorthHorizon.Common.Xmp
{
	/// <summary>
	/// Provides extension methods for <see cref="DependencyObject"/>.
	/// </summary>
	public static class DependencyObjectExtensions
	{
		private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

		/// <summary>
		/// Gets the value of a <see cref="DependencyProperty"/> from a specified
		/// <see cref="DependencyObject"/> on the UI thread, marshalling if ncessary.
		/// </summary>
		/// <param name="obj">The target <see cref="DependencyObject"/>.</param>
		/// <param name="dp">The desired <see cref="DependencyProperty"/>.</param>
		/// <returns>The value of the <see cref="DependencyProperty"/> on the <see cref="DependencyObject"/>.</returns>
		public static object SafeGetValue(this DependencyObject obj, DependencyProperty dp)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (dp == null)
				throw new ArgumentNullException("dp");

			if (obj.CheckAccess())
				return obj.GetValue(dp);

			var self = new Func<DependencyObject, DependencyProperty, object>(SafeGetValue);

			return Dispatcher.Invoke(self, obj, dp);
		}

		/// <summary>
		/// Sets the value of a <see cref="DependencyProperty"/> on a specified
		/// <see cref="DependencyObject"/> on the UI thread, marshalling if ncessary.
		/// </summary>
		/// <param name="obj">The target <see cref="DependencyObject"/>.</param>
		/// <param name="dp">The desired <see cref="DependencyProperty"/>.</param>
		/// <param name="value">The value to set.</param>
		public static void SafeSetValue(this DependencyObject obj, DependencyProperty dp, object value)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (dp == null)
				throw new ArgumentNullException("dp");

			if (obj.CheckAccess())
				obj.SetValue(dp, value);
			else
			{
				var self = new Action<DependencyObject, DependencyProperty, object>(SafeSetValue);
				Dispatcher.Invoke(self, obj, dp, value);
			}
		}

		/// <summary>
		/// Sets the value of a read-only <see cref="DependencyProperty"/> on a specified
		/// <see cref="DependencyObject"/> on the UI thread, marshalling if ncessary.
		/// </summary>
		/// <param name="obj">The target <see cref="DependencyObject"/>.</param>
		/// <param name="key">The desired <see cref="DependencyPropertyKey"/>.</param>
		/// <param name="value">The value to set.</param>
		public static void SafeSetValue(this DependencyObject obj, DependencyPropertyKey key, object value)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (key == null)
				throw new ArgumentNullException("key");

			if (obj.CheckAccess())
				obj.SetValue(key, value);
			else
			{
				var self = new Action<DependencyObject, DependencyPropertyKey, object>(SafeSetValue);
				Dispatcher.Invoke(self, obj, key, value);
			}
		}
	}
}
