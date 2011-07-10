namespace NorthHorizon.Common.Xmp.Composition
{
    /// <summary>
    /// Provides operations on all scoping constructs.
    /// </summary>
    public interface IContainerScope
    {
        /// <summary>
        /// Gets the name of the targetted container scope.
        /// </summary>
        /// <remarks>This is case-sensitive.</remarks>
        string ContainerScopeName { get; }
    }
}
