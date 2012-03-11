using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Autofac;
using MahTweets.Core.Extensions;

namespace MahTweets.Core.Composition
{
    public static class CompositionManager
    {
        private static ComposablePartCatalog _catalog;
        private static bool _isConfigured;

        private static IContainer _container;

        public static void ConfigureDependencies(ContainerBuilder builder, ComposablePartCatalog catalog)
        {
            if (_isConfigured)
                return;

            _catalog = catalog;
            _container = builder.Build();

            _isConfigured = true;
        }

        public static T Get<T>()
        {
            if (_container == null)
                return default(T);

            return _container.Resolve<T>();
        }

        public static IEnumerable<T> GetAll<T>()
        {
            if (_container == null)
                return new List<T>();

            return _container.Resolve<IEnumerable<T>>();
        }

        public static T Get<T>(Type type)
        {
            if (_container.IsRegistered(type))
            {
                object t = _container.Resolve(type);

                if (t is T)
                    return (T) t;
            }
            else
            {
                IEnumerable<T> allPossibleValues = GetAll<T>();
                return allPossibleValues.FirstOrDefault(t => t.GetType().Equals(type));
            }

            return default(T);
        }

        public static Type ResolveType<T>(string typeName, string namespaceName, params Type[] additionalTypes)
        {
            Type t = null;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(_catalog);
            catalog.Catalogs.Add(new TypeCatalog(additionalTypes));

            var container = new CompositionContainer(catalog);

            foreach (var c in container.GetExports<T>())
            {
                string assemblyQualifiedName = c.Value.GetType().AssemblyQualifiedName;
                if (assemblyQualifiedName != null)
                {
                    string[] currentAssemblyValues = assemblyQualifiedName.Split(new[] {','},
                                                                                 StringSplitOptions.RemoveEmptyEntries);

                    string compareTypeName = currentAssemblyValues[0].Trim();
                    string compareNamespaceName = currentAssemblyValues[1].Trim();

                    if (compareTypeName.Matches(typeName) && compareNamespaceName.Matches(namespaceName))
                        //string.CompareOrdinal(compareTypeName, typeName) == 0 && string.CompareOrdinal(compareNamespaceName, namespaceName) == 0)))
                    {
                        t = c.Value.GetType();
                        break;
                    }
                }
            }

            return t;
        }
    }
}