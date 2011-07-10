using System;
using System.Linq;

namespace NorthHorizon.Common.Xmp.Composition
{
    /// <summary>
    /// Provides scoping on namespaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class NamespaceCompositionContainerScopeAttribute : Attribute, IGlobalCompositionContainerScope
    {
        private readonly string[] _namespaceParts;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceCompositionContainerScopeAttribute"/> class.
        /// </summary>
        /// <param name="containerScopeName">The name of the container scope. Case-sensitive.</param>
        /// <param name="namespace">The namespace to which the scope is being applied. Case-sensitive.</param>
        public NamespaceCompositionContainerScopeAttribute(string containerScopeName, string @namespace)
        {
            if (string.IsNullOrEmpty(containerScopeName)) throw new ArgumentNullException("containerScopeName");
            if (string.IsNullOrEmpty(@namespace)) throw new ArgumentNullException("namespace");

            ContainerScopeName = containerScopeName;

            Namespace = @namespace;
            _namespaceParts = @namespace.Split('.');

            Priority = _namespaceParts.Length;
        }

        /// <summary>
        /// Determines whether the given type should be included in the scopee.
        /// </summary>
        /// <param name="prospectiveType">The type being queried.</param>
        /// <returns>
        ///   <c>True</c> if the type should be included; <c>false</c> if it should be excluded; or <c>null</c> if this scope is not applicable.
        /// </returns>
        public bool? ShouldIncludeInContainer(Type prospectiveType)
        {
            if (prospectiveType == null) throw new ArgumentNullException("prospectiveType");

            return IsInNamespace(prospectiveType.Namespace) ? !IsExcluded : (bool?)null;
        }

        /// <summary>
        /// Gets or sets the namespace being scoped.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the name of the targetted container scope.
        /// </summary>
        public string ContainerScopeName { get; private set; }

        /// <summary>
        /// Gets a value indicating the level of importance with which this scope should be applied.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this namespace should be excluded from the scope.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this namespace is excluded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExcluded { get; set; }

        private bool IsInNamespace(string prospectiveNamespace)
        {
            var prospectiveNamespaceParts = prospectiveNamespace.Split('.');

            return !(prospectiveNamespaceParts.Length < _namespaceParts.Length ||
                     _namespaceParts.Where((t, i) => !string.Equals(t, prospectiveNamespaceParts[i], StringComparison.Ordinal)).Any());
        }
    }
}