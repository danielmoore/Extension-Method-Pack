using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace NorthHorizon.Common.Xmp
{
#if !NET40

	/// <summary>
	/// Provides extensions for <see cref="Dispatcher"/> objects.
	/// </summary>
	public static class DispatcherExtensions
	{
		/// <summary>
		///		Executes the specified action synchronously
		///		on the thread the <see cref="Dispatcher"/> is associated with.
		/// </summary>
		/// <param name="dispatcher">The target dispatcher.</param>
		/// <param name="action">The action to execute.</param>
		public static void Invoke(this Dispatcher dispatcher, Action action)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");

			dispatcher.Invoke((Delegate)action);
		}

		public static T Invoke<T>(this Dispatcher dispatcher, Func<T> function)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");

			return (T)dispatcher.Invoke((Delegate)function);
		}

		public static void BeginInvoke(this Dispatcher dispatcher, Action action)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");

			dispatcher.BeginInvoke((Delegate)action);
		}

		public static void BeginInvoke(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
		{
			if (dispatcher == null)
				throw new ArgumentNullException("dispatcher");

			dispatcher.BeginInvoke((Delegate)action, priority);
		}
	}

#endif
}
