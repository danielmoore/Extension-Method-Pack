using System;

namespace NorthHorizon.Common.Xmp.Composition
{
    public interface IGlobalCompositionContainerScope
    {
        bool? ShouldIncludeInContainer(string prospectiveContainerScopeName, Type prospectiveType);
    }
}