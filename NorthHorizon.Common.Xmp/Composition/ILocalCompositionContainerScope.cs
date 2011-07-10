namespace NorthHorizon.Common.Xmp.Composition
{
    /// <summary>
    /// Provides scoping operations on a specific member.
    /// </summary>
    public interface ILocalCompositionContainerScope : IContainerScope
    {
        /// <summary>
        /// Determines whether the targetted member should be included in the container scope.
        /// </summary>
        /// <returns><c>True</c> if the type should be included; <c>false</c> if it should be excluded; or <c>null</c> if this scope is not applicable.</returns>
        bool? ShouldIncludeInContainer();
    }
}