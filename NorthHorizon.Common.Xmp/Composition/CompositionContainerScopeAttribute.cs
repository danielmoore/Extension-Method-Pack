using System;

namespace NorthHorizon.Common.Xmp.Composition
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CompositionContainerScopeAttribute : Attribute, ILocalCompositionContainerScope
    {
        private readonly string _containerScopeName;

        public CompositionContainerScopeAttribute(string containerScopeName)
        {
            _containerScopeName = containerScopeName;
        }

        public bool IsExcluded { get; set; }

        public bool? ShouldIncludeInContainer(string prospectiveContainerScopeName)
        {
            return string.Equals(_containerScopeName, prospectiveContainerScopeName, StringComparison.Ordinal) ? (bool?)!IsExcluded : null;
        }
    }
}
