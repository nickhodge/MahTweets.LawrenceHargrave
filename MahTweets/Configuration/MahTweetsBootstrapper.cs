using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using MahTweets.Core.Composition;

namespace MahTweets.Configuration
{
    public class MahTweetsBootstrapper
    {
        public MahTweetsBootstrapper()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyPath = Path.GetDirectoryName(assembly.Location);
            PluginsPath = AssemblyPath + Path.DirectorySeparatorChar + "Plugins";
        }

        public string AssemblyPath { get; private set; }

        public string PluginsPath { get; private set; }

        public void Bootstrap()
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(AssemblyPath));

            if (Directory.Exists(PluginsPath))
                catalog.Catalogs.Add(new DirectoryCatalog(PluginsPath));

            var dir = new DirectoryInfo(PluginsPath);
            if (dir.Exists)
            {
                foreach (var d in dir.GetDirectories())
                    catalog.Catalogs.Add(new DirectoryCatalog(d.FullName));
            }

            var registry = new MahTweetsBuilder(catalog);

            CompositionManager.ConfigureDependencies(registry, catalog);
        }

        public void Shutdown()
        {
        }
    }
}