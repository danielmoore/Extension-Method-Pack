using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthHorizon.Common.Xmp.Composition;

[assembly: CompositionContainerScope("bar")]
[assembly: NamespaceCompositionContainerScope("quoi", "NorthHorizon.Common.Xmp")]

namespace NorthHorizon.Common.Xmp.Tests
{
    [TestClass]
    public class CompositionContainerExtensionsTestFixture
    {
        [TestMethod]
        public void Test()
        {
            var compositionContainer = new CompositionContainer(new TypeCatalog(typeof(TestType)));

            var childContainer = compositionContainer.CreateChildContainer("foo");
        }

        [Export, CompositionContainerScope("foo")]
        private class TestType : IDisposable
        {
            [Export("TestPropName"), CompositionContainerScope("foo", IsExcluded = true)]
            public int TestProp { get { return 3; } }

            #region IDisposable Members

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}
