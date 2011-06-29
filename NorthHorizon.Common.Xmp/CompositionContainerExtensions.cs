using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using NorthHorizon.Common.Xmp.Composition;

namespace NorthHorizon.Common.Xmp
{
    public static class CompositionContainerExtensions
    {
        public static CompositionContainer CreateChildContainer(this CompositionContainer parentContainer, string containerName, bool inherit = true)
        {
            if (parentContainer == null) throw new ArgumentNullException("parentContainer");
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException("containerName");

            var catalog = new FilteredPartCatalog(parentContainer.Catalog, containerName);
            return inherit ? new CompositionContainer(catalog, parentContainer) : new CompositionContainer(catalog);
        }

        private class FilteredPartCatalog : ComposablePartCatalog, ICompositionElement
        {
            private readonly string _containerName;
            private readonly IQueryable<ComposablePartDefinition> _parts;
            private readonly ICompositionElement _origin;

            public FilteredPartCatalog(ComposablePartCatalog baseCatalog, string containerName)
            {
                _origin = baseCatalog as ICompositionElement;
                _containerName = containerName;

                _parts = baseCatalog
                    .Parts
                    .Select(p => GetPart(p))
                    .Where(p => p != null)
                    .AsQueryable();
            }

            public override IQueryable<ComposablePartDefinition> Parts { get { return _parts; } }

            private ComposablePartDefinition GetPart(ComposablePartDefinition sourcePart)
            {
                var exports = sourcePart
                    .ExportDefinitions
                    .Select(d => new { Definition = d, Info = ReflectionModelServices.GetExportingMember(d) })
                    .Where(e => IsIncluded(e.Info));

                var partType = ReflectionModelServices.GetPartType(sourcePart);
                var importDefinitions = new Lazy<IEnumerable<ImportDefinition>>(() => sourcePart.ImportDefinitions);
                var exportDefinitions = new Lazy<IEnumerable<ExportDefinition>>(() => exports.Select(e => e.Definition));
                var metadata = new Lazy<IDictionary<string, object>>(() => sourcePart.Metadata);

                var isDisposaleRequired = exports
                    .SelectMany(e => e.Info.GetAccessors())
                    .Where(m => m != null)
                    .Any(m => typeof(IDisposable).IsAssignableFrom(m as Type ?? m.DeclaringType));

                return exports.Any() ? ReflectionModelServices.CreatePartDefinition(partType, isDisposaleRequired, importDefinitions, exportDefinitions, metadata, this) : null;
            }

            private bool IsIncluded(LazyMemberInfo memberInfo)
            {
                var accessors = memberInfo.GetAccessors().Where(a => a != null);

                var types = accessors.Select(m => m as Type ?? m.DeclaringType);
                var assemblies = types.Select(t => t.Assembly).Distinct();

                var asmLocalScopes = assemblies
                    .SelectMany(Attribute.GetCustomAttributes)
                    .OfType<ILocalCompositionContainerScope>()
                    .Select(s => s.ShouldIncludeInContainer(_containerName));

                var asmGlobalScopes = assemblies
                    .SelectMany(Attribute.GetCustomAttributes)
                    .OfType<IGlobalCompositionContainerScope>()
                    .SelectMany(s => types.Select(t => s.ShouldIncludeInContainer(_containerName, t)));

                var accessorAttr = accessors.SelectMany(Attribute.GetCustomAttributes);
                accessorAttr.ToList();

                var localScopes = accessors
                    .Concat(accessors.GroupJoin(
                        types.SelectMany(t => t.GetProperties()),
                        a => a,
                        a => a.GetGetMethod(),
                        (l, r) => r.SingleOrDefault()).Where(a => a != null))
                    .SelectMany(Attribute.GetCustomAttributes)
                    .OfType<ILocalCompositionContainerScope>()
                    .Select(s => s.ShouldIncludeInContainer(_containerName));

                var scopes = asmLocalScopes.Concat(asmGlobalScopes).Concat(localScopes).Where(s => s.HasValue);

                return scopes.Any() && scopes.All(s => s.Value);
            }

            string ICompositionElement.DisplayName { get { return typeof(FilteredPartCatalog).FullName; } }

            ICompositionElement ICompositionElement.Origin { get { return _origin; } }
        }
    }
}
