using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Pronium.Configuration;
#if __EMBY__
using System.IO;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Drawing;
#else
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Sentry;
#endif

[assembly: CLSCompliant(false)]

namespace Pronium
{
#if __EMBY__
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
#else
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
#endif
    {
#if __EMBY__
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IHttpClient http, ILogManager logger)
#else
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IHttpClientFactory http, ILogger<Plugin> logger)
#endif
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            Http = http;

#if __EMBY__
            if (logger != null)
            {
                Log = logger.GetLogger(Name);
            }
#else
            Log = logger;
            ConfigurationChanged += PluginConfiguration.ConfigurationChanged;

            SentrySdk.Init(new SentryOptions { Dsn = Consts.SentryDSN });
#endif
        }

#if __EMBY__
        public static IHttpClient Http { get; set; }
#else
        public static IHttpClientFactory Http { get; set; }
#endif

        public static ILogger Log { get; set; }

        public static Plugin Instance { get; private set; }

        public override string Name => "Pronium";

        public override Guid Id => Guid.Parse("172ff6fc-2297-4c96-979a-e0ad632b6120");

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo { Name = Name, EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html" },
            };
        }

#if __EMBY__
        public Stream GetThumbImage()
        {
            return GetType().Assembly.GetManifestResourceStream($"{GetType().Namespace}.logo.png");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;
#endif
    }
}
