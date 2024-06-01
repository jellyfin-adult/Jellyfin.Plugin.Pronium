using System.Reflection;
using Pronium.Helpers;

namespace Pronium
{
    public static class Consts
    {
        public const string DatabaseUpdateURL = "https://api.github.com/repos/jellyfin-adult/Jellyfin.Plugin.Pronium/contents/data";

#if __EMBY__
        public const string PluginInstance = "Emby.Plugins.Pronium";
#else
        public const string PluginInstance = "Jellyfin.Plugin.Pronium";
#endif

        public static readonly string PluginVersion = $"{Plugin.Instance.Version} build {Helper.GetLinkerTime(Assembly.GetAssembly(typeof(Plugin)))}";
    }
}
