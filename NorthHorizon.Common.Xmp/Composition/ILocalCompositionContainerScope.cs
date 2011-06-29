namespace NorthHorizon.Common.Xmp.Composition
{
    public interface ILocalCompositionContainerScope
    {
        bool? ShouldIncludeInContainer(string prospectiveContainerScopeName);
    }
}