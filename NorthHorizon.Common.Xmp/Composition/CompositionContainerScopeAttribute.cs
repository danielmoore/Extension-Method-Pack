using System;

namespace NorthHorizon.Common.Xmp.Composition
{
    /// <summary>
    /// Provides scoping on specific members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CompositionContainerScopeAttribute : Attribute, ILocalCompositionContainerScope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionContainerScopeAttribute"/> class.
        /// </summary>
        /// <param name="containerScopeName">The name of the container scope. Case-sensitive.</param>
        public CompositionContainerScopeAttribute(string containerScopeName)
        {
            if (string.IsNullOrEmpty(containerScopeName)) throw new ArgumentNullException("containerScopeName");

            ContainerScopeName = containerScopeName;
        }

        /// <summary>
        /// Gets the name of the targetted container scope.
        /// </summary>
        public string ContainerScopeName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this member should be excluded from the scope.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this member is excluded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExcluded { get; set; }

        /// <summary>
        /// Determines whether the targetted member should be included in the container scope.
        /// </summary>
        /// <returns>
        ///   <c>True</c> if the type should be included; <c>false</c> if it should be excluded; or <c>null</c> if this scope is not applicable.
        /// </returns>
        public bool? ShouldIncludeInContainer()
        {
            return !IsExcluded;
        }
    }
}
