using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using PhoenixAdultRebirth.Helpers;
using PhoenixAdultRebirth.Helpers.Utils;

namespace PhoenixAdultRebirth.Sites
{
    public class SiteData18 : IProviderBase
    {
        public async Task<List<RemoteSearchResult>> Search(int[] siteNum, string searchTitle, DateTime? searchDate, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();
            if (siteNum == null || string.IsNullOrEmpty(searchTitle))
            {
                return result;
            }

            var url = Helper.GetSearchSearchURL(siteNum) + searchTitle;
            Logger.Info($"SiteData18.Search url: {url}");
            var cookies = new Dictionary<string, string> { { "data_user_captcha", "1" } };
            var data = await HTML.ElementFromURL(url, cancellationToken, null, cookies).ConfigureAwait(false);

            var searchResults = data.SelectNodesSafe("//a");
            Logger.Info($"SiteData18.Search searchResults: {searchResults.Count}");
            foreach (var searchResult in searchResults)
            {
                var sceneURL = new Uri(searchResult.Attributes["href"].Value);
                var curID = Helper.Encode(sceneURL.AbsoluteUri);
                var sceneName = searchResult.SelectSingleText(".//p[@class='gen12']");
                var scenePoster = searchResult.SelectSingleText(".//img/@src");
                var sceneDate = searchResult.SelectSingleText(".//span[@class='gen11'] | ./text()");

                var res = new RemoteSearchResult { Name = sceneName, ImageUrl = scenePoster, };

                if (!string.IsNullOrEmpty(sceneDate))
                {
                    sceneDate = sceneDate.Replace("&nbsp;", string.Empty).Trim();
                }

                Logger.Info($"SiteData18.Search sceneURL: {sceneURL}, ID: {curID}, sceneName: {sceneName}, scenePoster: {scenePoster}, sceneDate: {sceneDate}");

                if (DateTime.TryParse(sceneDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
                {
                    res.PremiereDate = sceneDateObj;
                }

                Logger.Info($"SiteData18.Search date: {res.PremiereDate.ToString()}");

                if (res.PremiereDate.HasValue)
                {
                    curID += $"#{res.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
                }

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

            var sceneURL = Helper.Decode(sceneID[0]);
            var sceneDate = string.Empty;

            Logger.Info($"SiteData18.Update sceneID: {sceneID[0]}, sceneURL: {sceneURL}");

            if (sceneID.Length > 1)
            {
                sceneDate = sceneID[1];
            }

            var headers = new Dictionary<string, string> { { "Cookie", "data_user_captcha=1" } };
            var sceneData = await HTML.ElementFromURL(sceneURL, cancellationToken, headers).ConfigureAwait(false);
            var name = sceneData.SelectSingleText("//h1/a");
            var overview = sceneData.SelectSingleText("//div[@class='gen12']/div[contains(., 'Description') or contains(., 'Story')]");

            Logger.Info($"SiteData18.Update name: {name}, sceneDate: {sceneDate}");
            result.Item.ExternalId = sceneURL;

            result.Item.Name = name;
            result.Item.Overview = overview
                .Replace("&nbsp;", " ")
                .Replace("Description - ", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("Story - ", string.Empty, StringComparison.OrdinalIgnoreCase);

            result.Item.AddStudio("Data18");
            var studios = sceneData.SelectNodesSafe("//div[@class='gen12']/p[contains(., 'Network') or contains(., 'Site')]//a");
            foreach (var studio in studios)
            {
                var studioName = studio.InnerText;

                if (!string.IsNullOrEmpty(studioName))
                {
                    result.Item.AddStudio(studioName);
                }
            }

            if (!string.IsNullOrEmpty(sceneDate))
            {
                if (DateTime.TryParseExact(sceneDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
                {
                    result.Item.PremiereDate = sceneDateObj;
                }
            }
            else
            {
                var date = sceneData.SelectSingleText("//span[contains(., 'Release date:') and not(contains(., 'No date release yet'))]/a");
                if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, "MMMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
                {
                    result.Item.PremiereDate = sceneDateObj;
                }
            }

            var genreNode = sceneData.SelectNodesSafe("//*[contains(., 'Categories:')]/a");
            foreach (var genreLink in genreNode)
            {
                var genreName = genreLink.InnerText;

                result.Item.AddGenre(genreName);
            }

            var actorsNode = sceneData.SelectNodesSafe("//h3[contains(., 'Cast')]/parent::div//img");
            foreach (var actorLink in actorsNode)
            {
                var actor = new PersonInfo { Name = actorLink.SelectSingleText("./@alt"), ImageUrl = actorLink.SelectSingleText("./@src"), };

                result.People.Add(actor);
            }

            Logger.Info($"SiteData18.Update genres: {result.Item.Genres.Length}, actors: {result.People.Count}");

            return result;
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(int[] siteNum, string[] sceneID, BaseItem item, CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (sceneID == null)
            {
                return result;
            }

            var sceneURL = Helper.Decode(sceneID[0]);
            if (!sceneURL.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                sceneURL = Helper.GetSearchBaseURL(siteNum) + sceneURL;
            }

            var sceneData = await HTML.ElementFromURL(sceneURL, cancellationToken).ConfigureAwait(false);

            foreach (var node in sceneData.SelectNodesSafe("//a[@data-featherlight='image']"))
            {
                var img = node.SelectSingleText("./@href");

                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Primary, });
                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop, });
            }

            return result;
        }
    }
}
