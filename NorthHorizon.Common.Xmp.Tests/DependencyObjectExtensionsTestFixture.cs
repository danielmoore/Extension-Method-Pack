using System;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Common.Xmp.Tests
{
	[TestClass]
	public class DependencyObjectExtensionsTestFixture
	{
		private static readonly DependencyPropertyKey TestReadOnlyValuePropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("TestReadOnlyValue", typeof(int),
			typeof(DependencyObjectExtensionsTestFixture), new PropertyMetadata());
		public static readonly DependencyProperty TestReadOnlyValueProperty =
			TestReadOnlyValuePropertyKey.DependencyProperty;

		private static Application _app;
		private static Thread _appThread;
		private Window _window;
		private static AutoResetEvent _appStartSync;

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			_appStartSync = new AutoResetEvent(false);
			_appThread = new Thread(StartApp);
			_appThread.SetApartmentState(ApartmentState.STA);
			_appThread.Start();

			_appStartSync.WaitOne();
		}

		private static void StartApp()
		{
			_app = new Application();
			_appStartSync.Set();
			_app.Run();
		}

		[TestInitialize]
		public void TestInititalize()
		{
			_app.Dispatcher.Invoke(new Action(() => _window = new Window { Height = 30, Width = 30 }));
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestAccessException()
		{
			// This is actually a test of the tests... We need to make sure we're on
			// a different thread and accessing the UI object will throw an exception.

			_window.Height = 50;
		}

		[TestMethod]
		public void TestSafeGetValue()
		{
			var value = _window.SafeGetValue(Window.HeightProperty);
			Assert.AreEqual(30d, value);
		}

		[TestMethod]
		public void TestSafeSetValue()
		{
			_window.SafeSetValue(Window.HeightProperty, 60d);
			var value = _window.SafeGetValue(Window.HeightProperty);
			Assert.AreEqual(60d, value);
		}

		[TestMethod]
		public void TestSafeSetValuePropertyKey()
		{
			_window.SafeSetValue(TestReadOnlyValuePropertyKey, 42);
			var value = _window.SafeGetValue(TestReadOnlyValueProperty);
			Assert.AreEqual(42, value);
		}
	}
}
