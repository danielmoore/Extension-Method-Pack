using System;

namespace NorthHorizon.Common.Xmp.Composition
{
    /// <summary>
    /// Provides scoping operations over any type.
    /// </summary>
    public interface IGlobalCompositionContainerScope : IContainerScope
    {
        /// <summary>
        /// Gets a value indicating the level of importance with which this scope should be applied.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Determines whether the given type should be included in the scopee.
        /// </summary>
        /// <param name="prospectiveType">The type being queried.</param>
        /// <returns><c>True</c> if the type should be included; <c>false</c> if it should be excluded; or <c>null</c> if this scope is not applicable.</returns>
        bool? ShouldIncludeInContainer(Type prospectiveType);
    }
}