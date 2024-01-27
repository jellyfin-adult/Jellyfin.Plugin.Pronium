using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using PhoenixAdultRebirth.Helpers;
using PhoenixAdultRebirth.Helpers.Utils;

namespace PhoenixAdultRebirth.Sites
{
    public class SitePornWorld : IProviderBase
    {
        public async Task<List<RemoteSearchResult>> Search(int[] siteNum, string searchTitle, DateTime? searchDate, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();
            if (siteNum == null || string.IsNullOrEmpty(searchTitle))
            {
                return result;
            }

            var url = Helper.GetSearchSearchURL(siteNum) + searchTitle;
            Logger.Info($"SitePornWorld.Search url: {url}");
            var data = await HTML.ElementFromURL(url, cancellationToken).ConfigureAwait(false);

            var searchResults = data.SelectNodesSafe("//div[@class='col d-flex']");
            Logger.Info($"SitePornWorld.Search searchResults: {searchResults.Count}");
            foreach (var searchResult in searchResults)
            {
                var sceneUrl = new Uri(searchResult.SelectSingleNode(".//a").Attributes["href"].Value);
                var curID = Helper.Encode(sceneUrl.AbsoluteUri);
                var sceneName = searchResult.SelectSingleText(".//div[@class='card-scene__text']");
                var scenePoster = searchResult.SelectSingleText(".//img/@src");
                var res = new RemoteSearchResult { Name = sceneName, ImageUrl = scenePoster, };

                Logger.Info($"SitePornWorld.Search sceneURL: {sceneUrl}, ID: {curID}, sceneName: {sceneName}, scenePoster: {scenePoster}");

                res.ProviderIds.Add(Plugin.Instance?.Name ?? "PhoenixAdultRebirth", curID);

                result.Add(res);
            }

            return result;
        }

        public async Task<MetadataResult<BaseItem>> Update(int[] siteNum, string[] sceneID, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<BaseItem>() { Item = new Movie(), People = new List<PersonInfo>(), };

            if (sceneID == null)
            {
                return result;
            }

            var sceneUrl = Helper.Decode(sceneID[0]);

            Logger.Info($"SitePornWorld.Update sceneID: {sceneID[0]}, sceneURL: {sceneUrl}");

            var sceneData = await HTML.ElementFromURL(sceneUrl, cancellationToken).ConfigureAwait(false);
            var name = sceneData.SelectSingleText("//h1").Split("featuring")[0];
            var overview = sceneData.SelectSingleText("//div[@class='text-mob-more p-md']");

            Logger.Info($"SitePornWorld.Update name: {name}");
            result.Item.ExternalId = sceneUrl;

            result.Item.Name = name;
            result.Item.Overview = overview;
            result.Item.AddStudio("PornWorld");

            var date = sceneData.SelectSingleText("//i[@class='bi bi-calendar3 me-5']//parent::div");
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
            {
                result.Item.PremiereDate = sceneDateObj;
            }

            var genreNode = sceneData.SelectNodesSafe("//div[contains(@class, 'genres-list')]/a");
            foreach (var genreLink in genreNode)
            {
                var genreName = genreLink.InnerText;

                result.Item.AddGenre(genreName);
            }

            var actorsNode = sceneData.SelectNodesSafe("//h1//a");
            foreach (var actorLink in actorsNode)
            {
                var actor = new PersonInfo { Name = actorLink.InnerText };

                result.People.Add(actor);
            }

            Logger.Info($"SitePornWorld.Update genres: {result.Item.Genres.Length}, actors: {result.People.Count}");

            return result;
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(int[] siteNum, string[] sceneId, BaseItem item, CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (sceneId == null)
            {
                return result;
            }

            var sceneUrl = Helper.Decode(sceneId[0]);
            var sceneData = await HTML.ElementFromURL(sceneUrl, cancellationToken).ConfigureAwait(false);
            var primaryImage = sceneData.SelectSingleNode("//video");

            if (primaryImage != null)
            {
                var img = HttpUtility.HtmlDecode(primaryImage.Attributes["data-poster"].Value);

                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Primary, });
                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop, });
            }

            return result;
        }
    }
}
