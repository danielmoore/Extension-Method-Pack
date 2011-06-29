using System;
using System.Linq;

namespace NorthHorizon.Common.Xmp.Composition
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class NamespaceCompositionContainerScopeAttribute : Attribute, IGlobalCompositionContainerScope
    {
        private readonly string _containerScopeName;
        private readonly string _namespace;
        private readonly string[] _namespaceParts;

        public NamespaceCompositionContainerScopeAttribute(string containerScopeName, string @namespace)
        {
            _containerScopeName = containerScopeName;
            _namespace = @namespace;
            _namespaceParts = _namespace.Split('.');
        }

        public bool? ShouldIncludeInContainer(string prospectiveContainerScopeName, Type prospectiveType)
        {
            if (string.Equals(_containerScopeName, prospectiveContainerScopeName, StringComparison.Ordinal))
                return !IsExcluded && IsInNamespace(prospectiveType.Namespace);
            else
                return null;
        }

        public bool IsExcluded { get; set; }

        private bool IsInNamespace(string prospectiveNamespace)
        {
            var prospectiveNamespaceParts = prospectiveNamespace.Split('.');

            return !(prospectiveNamespaceParts.Length < _namespaceParts.Length ||
                     _namespaceParts.Where((t, i) => !string.Equals(t, prospectiveNamespaceParts[i], StringComparison.Ordinal)).Any());
        }
    }
}