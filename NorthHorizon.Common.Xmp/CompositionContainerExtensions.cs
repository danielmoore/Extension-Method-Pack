using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NorthHorizon.Common.Xmp.Composition;

namespace NorthHorizon.Common.Xmp
{
    /// <summary>
    /// Provides extensions to <see cref="CompositionContainer"/>.
    /// </summary>
    public static class CompositionContainerExtensions
    {
        private static readonly ConditionalWeakTable<ComposablePartCatalog, IList<WeakReference>> KnownCatalogs =
            new ConditionalWeakTable<ComposablePartCatalog, IList<WeakReference>>();

        /// <summary>
        /// Creates a child container from a given parent container within the given scope.
        /// </summary>
        /// <param name="parentContainer">The parent container from which to derive the child container.</param>
        /// <param name="containerName">The name of the container to build.</param>
        /// <param name="inherit">A value indicating whether the inherit values from the parent container if not available in the child container.</param>
        /// <returns>A child container scoped by its name.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "managed externally")]
        public static CompositionContainer CreateChildContainer(this CompositionContainer parentContainer, string containerName, bool inherit = true)
        {
            if (parentContainer == null) throw new ArgumentNullException("parentContainer");
            if (string.IsNullOrEmpty(containerName)) throw new ArgumentNullException("containerName");

            var catalog = GetScopedCatalog(parentContainer.Catalog, containerName);

            return inherit ? new CompositionContainer(catalog, parentContainer) : new CompositionContainer(catalog);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "managed externally")]
        private static ContainerScopedPartCatalog GetScopedCatalog(ComposablePartCatalog baseCatalog, string containerName)
        {
            var openCatalogs = KnownCatalogs.GetValue(baseCatalog, c => new List<WeakReference>());

            lock (openCatalogs)
            {
                for (int i = openCatalogs.Count - 1; i >= 0; i--)
                {
                    var catalog = (ContainerScopedPartCatalog)openCatalogs[i].Target;
                    if (catalog == null)
                        openCatalogs.RemoveAt(i);
                    else if (string.Equals(containerName, catalog.ContainerName, StringComparison.Ordinal))
                        return catalog;
                }

                var newCatalog = new ContainerScopedPartCatalog(baseCatalog, containerName);

                openCatalogs.Add(new WeakReference(newCatalog));

                return newCatalog;
            }
        }

        private class ContainerScopedPartCatalog : ComposablePartCatalog, ICompositionElement, INotifyComposablePartCatalogChanged
        {
            public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed = delegate { };
            public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing = delegate { };

            private static readonly PropertyLookupCache PropertyLookupCache = new PropertyLookupCache();

            private readonly ICollection<ComposablePartDefinition> _parts;
            private readonly ICompositionElement _origin;

            private readonly INotifyComposablePartCatalogChanged _changingBaseCatalog;
            private readonly IDictionary<ComposablePartDefinition, ComposablePartDefinition> _baseDefinitionMap;

            public ContainerScopedPartCatalog(ComposablePartCatalog baseCatalog, string containerName)
            {
                ContainerName = containerName;
                _origin = baseCatalog as ICompositionElement;

                var baseCatalogPartCount = baseCatalog.Parts.Count();
                _parts = new List<ComposablePartDefinition>(baseCatalogPartCount);

                var changingBaseCatalog = baseCatalog as INotifyComposablePartCatalogChanged;
                if (changingBaseCatalog != null)
                {
                    _baseDefinitionMap = new Dictionary<ComposablePartDefinition, ComposablePartDefinition>(baseCatalogPartCount);

                    foreach (var part in baseCatalog.Parts)
                    {
                        var filteredPart = GetFilteredPart(part);
                        if (filteredPart != null)
                        {
                            _parts.Add(filteredPart);
                            _baseDefinitionMap[part] = filteredPart;
                        }
                    }

                    changingBaseCatalog.Changing += OnBaseCatalogChanging;

                    _changingBaseCatalog = changingBaseCatalog;
                }
                else
                {
                    baseCatalog.Parts.Select(GetFilteredPart).Where(p => p != null).ForEach(_parts.Add);
                }
            }

            public string ContainerName { get; private set; }

            string ICompositionElement.DisplayName { get { return typeof(ContainerScopedPartCatalog).FullName; } }

            ICompositionElement ICompositionElement.Origin { get { return _origin; } }

            private void OnBaseCatalogChanging(object sender, ComposablePartCatalogChangeEventArgs e)
            {
                var removedDefinitions = new List<ComposablePartDefinition>();

                foreach (var definition in e.RemovedDefinitions)
                {
                    ComposablePartDefinition filteredDefinition;
                    if (_baseDefinitionMap.TryGetValue(definition, out filteredDefinition))
                        removedDefinitions.Add(filteredDefinition);
                }

                var addedDefinitions = new List<ComposablePartDefinition>();

                foreach (var definition in e.AddedDefinitions)
                {
                    var filteredDefinition = GetFilteredPart(definition);
                    if (filteredDefinition != null)
                    {
                        addedDefinitions.Add(filteredDefinition);

                        if (_baseDefinitionMap != null)
                            _baseDefinitionMap[definition] = filteredDefinition;
                    }
                }

                var args = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removedDefinitions, null);

                Changing(this, args);

                _parts.RemoveAll(removedDefinitions);
                _parts.AddAll(addedDefinitions);

                Changed(this, args);
            }

            public override IQueryable<ComposablePartDefinition> Parts { get { return _parts.AsQueryable(); } }

            private ComposablePartDefinition GetFilteredPart(ComposablePartDefinition sourcePart)
            {
                var exports = sourcePart
                    .ExportDefinitions
                    .Select(d => new { Definition = d, Info = ReflectionModelServices.GetExportingMember(d) })
                    .Where(e => IsIncluded(e.Info));

                if (!exports.Any()) return null;

                var partType = ReflectionModelServices.GetPartType(sourcePart);
                var importDefinitions = new Lazy<IEnumerable<ImportDefinition>>(() => sourcePart.ImportDefinitions);
                var exportDefinitions = new Lazy<IEnumerable<ExportDefinition>>(() => exports.Select(e => e.Definition));
                var metadata = new Lazy<IDictionary<string, object>>(() => sourcePart.Metadata);

                var isDisposaleRequired = exports
                    .SelectMany(e => e.Info.GetAccessors())
                    .Where(m => m != null)
                    .Any(m => typeof(IDisposable).IsAssignableFrom(m as Type ?? m.DeclaringType));

                return ReflectionModelServices.CreatePartDefinition(partType, isDisposaleRequired, importDefinitions, exportDefinitions, metadata, this);
            }

            private bool IsIncluded(LazyMemberInfo memberInfo)
            {
                var accessors = memberInfo.GetAccessors().Where(a => a != null);

                var types = accessors.Select(m => m as Type ?? m.DeclaringType);
                var assemblies = types.Select(t => t.Assembly).Distinct().Where(a => a != null);

                // If we're a property, we need to also include the overarching PropertyInfos, 
                // since our accessor will be pointing to the getter method.
                var localAttributeTargets = memberInfo.MemberType == MemberTypes.Property ? accessors.Concat(PropertyLookupCache.Query(accessors)) : accessors;

                var scopeSets = new[] 
                {
                    // Local Scopes
                   localAttributeTargets
                        .SelectMany(Attribute.GetCustomAttributes)
                        .OfType<ILocalCompositionContainerScope>()
                        .Where(IsInScope)
                        .Select(s => s.ShouldIncludeInContainer()),

                    // Type Scopes
                    accessors
                        .Select(a => a.DeclaringType)
                        .Where(t => t != null)
                        .SelectMany(Attribute.GetCustomAttributes)
                        .OfType<ILocalCompositionContainerScope>()
                        .Where(IsInScope)
                        .Select(s => s.ShouldIncludeInContainer()),

                    // Assembly Global Scopes
                    assemblies
                        .SelectMany(Attribute.GetCustomAttributes)
                        .OfType<IGlobalCompositionContainerScope>()
                        .Where(IsInScope)
                        .GroupBy(s => s.Priority)
                        .OrderByDescending(g => g.Key)
                        .Select(g => g
                            .SelectMany(s => types.Select(s.ShouldIncludeInContainer))
                            .Aggregate((bool?)null, Conjoin)),

                    // Assembly Local Scopes
                    assemblies
                        .SelectMany(Attribute.GetCustomAttributes)
                        .OfType<ILocalCompositionContainerScope>()
                        .Where(IsInScope)
                        .Select(s => s.ShouldIncludeInContainer())
                };

                var result = scopeSets.Select(s => s.Aggregate((bool?)null, Conjoin)).FirstOrDefault(v => v.HasValue);

                return result ?? false;
            }

            private bool IsInScope(IContainerScope scope)
            {
                return string.Equals(ContainerName, scope.ContainerScopeName, StringComparison.Ordinal);
            }

            private static bool? Conjoin(bool? left, bool? right)
            {
                if (left == null) return right;
                if (right == null) return left;

                return left.Value && right.Value;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (_changingBaseCatalog != null)
                        _changingBaseCatalog.Changing -= OnBaseCatalogChanging;
                }
            }
        }

        private class PropertyLookupCache
        {
            private readonly ConcurrentDictionary<Type, IDictionary<MemberInfo, PropertyInfo>> _backingStore =
                new ConcurrentDictionary<Type, IDictionary<MemberInfo, PropertyInfo>>();

            private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty;

            public IEnumerable<PropertyInfo> Query(IEnumerable<MemberInfo> methods)
            {
                return methods
                    .GroupBy(m => m.DeclaringType)
                    .SelectMany(g =>
                    {
                        var map = _backingStore.GetOrAdd(g.Key, GetPropertyMap);
                        return g.Select(v => map[v]);
                    });
            }

            private static IDictionary<MemberInfo, PropertyInfo> GetPropertyMap(Type target)
            {
                return target
                    .GetProperties(PropertyBindingFlags)
                    .ToDictionary(p => (MemberInfo)p.GetGetMethod(), p => p);
            }
        }
    }
}
