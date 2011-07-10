using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthHorizon.Common.Xmp.Composition;

[assembly: CompositionContainerScope("bar")]
[assembly: NamespaceCompositionContainerScope("quoi", "NorthHorizon.Common.Xmp")]
[assembly: NamespaceCompositionContainerScope("quoi", "NorthHorizon.Common.Xmp.Tests.TestNamespace", IsExcluded = true)]

namespace NorthHorizon.Common.Xmp.Tests
{
    [TestClass]
    public class CompositionContainerExtensionsTestFixture
    {
        private const string FooScopeName = "foo";
        private const string BarScopeName = "bar";
        private const string QuoiScopeName = "quoi";
        private const string IncludedPropName = "IncludedTestPropName";
        private const string ExcludedPropname = "ExcludedTestPropName";

        private CompositionContainer _parentContainer;
        private CompositionContainer _fooContainer;
        private CompositionContainer _quoiContainer;
        private CompositionContainer _barContainer;

        [TestInitialize]
        public void Init()
        {
            var types = new List<Type>();

            AddNestedTypes(typeof(CompositionContainerExtensionsTestFixture), types);

            types.Add(typeof(TestNamespace.TestType));
            AddNestedTypes(typeof(TestNamespace.TestType), types);

            _parentContainer = new CompositionContainer(new TypeCatalog(types));

            _fooContainer = _parentContainer.CreateChildContainer(FooScopeName);
            _barContainer = _parentContainer.CreateChildContainer(BarScopeName);
            _quoiContainer = _parentContainer.CreateChildContainer(QuoiScopeName);
        }

        private static void AddNestedTypes(Type parent, IList<Type> foundTypes)
        {
            foreach (var type in parent.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                foundTypes.Add(type);
                AddNestedTypes(type, foundTypes);
            }
        }

        [TestMethod]
        public void TestDispose()
        {
            var testType = _fooContainer.GetExportedValue<TestType>();
            testType.IsDisposed = false;

            _fooContainer.Dispose();

            Assert.IsTrue(testType.IsDisposed);
        }

        [TestMethod]
        public void TestCatalogReuse()
        {
            Assert.AreSame(_fooContainer.Catalog, _parentContainer.CreateChildContainer(FooScopeName).Catalog);
        }

        [TestMethod]
        public void TestIncluded()
        {
            AreNotSame<object>(_fooContainer, _parentContainer, IncludedPropName);
        }

        [TestMethod]
        public void TestExcluded()
        {
            AreSame<object>(_fooContainer, _parentContainer, ExcludedPropname);
        }

        [TestMethod]
        public void TestNamespaceIncluded()
        {
            AreNotSame<object>(_quoiContainer, _parentContainer, IncludedPropName);
        }

        [TestMethod]
        public void TestNamespaceExcluded()
        {
            AreSame<object>(_quoiContainer, _parentContainer, ExcludedPropname);
        }

        [TestMethod]
        public void TestAssemblyIncluded()
        {
            AreNotSame<object>(_barContainer, _parentContainer, IncludedPropName);
        }

        [TestMethod]
        public void TestAssemblyExcluded()
        {
            AreSame<object>(_barContainer, _parentContainer, ExcludedPropname);
        }

        [TestMethod]
        public void TestScopeInheritance()
        {
            AreSame<TestType2.ChildTestType>(_barContainer, _parentContainer);
        }

        [TestMethod]
        public void TestNamespaceScopeInheritance()
        {
            AreSame<TestNamespace.TestType>(_quoiContainer, _parentContainer);
        }

        private static void AreSame<T>(CompositionContainer leftScope, CompositionContainer rightScope, string contractName = null)
        {
            TestScope<T>(leftScope, rightScope, contractName, Assert.AreSame);
        }

        private static void AreNotSame<T>(CompositionContainer leftScope, CompositionContainer rightScope, string contractName = null)
        {
            TestScope<T>(leftScope, rightScope, contractName, Assert.AreNotSame);
        }

        private static void TestScope<T>(CompositionContainer leftScope, CompositionContainer rightScope, string contractName, Action<object, object> assertion)
        {
            var errors = new List<Exception>();
            T left = default(T), right = default(T);

            try
            {
                left = leftScope.GetExportedValue<T>(contractName);
            }
            catch (Exception error)
            {
                errors.Add(new Exception("Failed to create left side", error));
            }

            try
            {
                right = rightScope.GetExportedValue<T>(contractName);
            }
            catch (Exception error)
            {
                errors.Add(new Exception("Failed to create right side", error));
            }

            if (errors.Any())
                throw new AggregateException("Errors resolving components", errors);

            assertion(left, right);
        }

        [Export, CompositionContainerScope(FooScopeName)]
        private class TestType : IDisposable
        {
            [Export(ExcludedPropname)]
            [CompositionContainerScope(FooScopeName, IsExcluded = true)]
            [CompositionContainerScope(BarScopeName, IsExcluded = true)]
            [CompositionContainerScope(QuoiScopeName, IsExcluded = true)]
            public readonly object ExcludedTestProp = new object();

            [Export(IncludedPropName)]
            public readonly object IncludedTestProp = new object();

            public bool IsDisposed { get; set; }

            public void Dispose() { IsDisposed = true; }
        }

        [Export, CompositionContainerScope(BarScopeName, IsExcluded = true)]
        private class TestType2
        {
            [Export]
            public class ChildTestType { }
        }
    }

    namespace TestNamespace
    {
        [Export]
        public class TestType { }
    }
}
