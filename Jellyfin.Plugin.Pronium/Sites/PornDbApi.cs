using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Newtonsoft.Json.Linq;
using Pronium.Helpers;
using Pronium.Helpers.Utils;

namespace Pronium.Sites
{
    public class PornDbApi : IProviderBase
    {
        public async Task<List<RemoteSearchResult>> Search(
            int[] siteNum,
            string searchTitle,
            DateTime? searchDate,
            CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();
            if (siteNum == null || string.IsNullOrEmpty(searchTitle))
            {
                return result;
            }

            if (searchDate.HasValue)
            {
                searchTitle += searchDate.Value.ToString("yyyy-MM-dd");
            }

            var url = Helper.GetSearchSearchURL(siteNum) + $"/scenes?parse={searchTitle}";
            var searchResults = await GetDataFromApi(url, cancellationToken).ConfigureAwait(false);
            if (searchResults == null)
            {
                return result;
            }

            foreach (var (idx, searchResult) in searchResults["data"].WithIndex())
            {
                string curID = (string)searchResult["_id"],
                    sceneName = (string)searchResult["title"],
                    sceneDate = (string)searchResult["date"],
                    scenePoster = (string)searchResult["poster"];

                var res = new RemoteSearchResult
                {
                    ProviderIds = { { Plugin.Instance?.Name ?? "Pronium", curID } }, Name = sceneName, ImageUrl = scenePoster, IndexNumberEnd = idx,
                };

                if (DateTime.TryParseExact(
                        sceneDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var sceneDateObj))
                {
                    res.PremiereDate = sceneDateObj;
                }

                result.Add(res);
            }

            return result;
        }

        public async Task<MetadataResult<BaseItem>> Update(int[] siteNum, string[] sceneID, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<BaseItem> { Item = new Movie(), People = new List<PersonInfo>() };

            if (sceneID == null)
            {
                return result;
            }

            var url = Helper.GetSearchSearchURL(siteNum) + $"/scenes/{sceneID[0]}";
            var sceneData = await GetDataFromApi(url, cancellationToken).ConfigureAwait(false);
            if (sceneData == null)
            {
                return result;
            }

            sceneData = (JObject)sceneData["data"];
            var sceneURL = Helper.GetSearchBaseURL(siteNum) + $"/scenes/{sceneID[0]}";

            result.Item.ExternalId = sceneURL;

            result.Item.Name = (string)sceneData["title"];
            result.Item.Overview = (string)sceneData["description"];
            if (sceneData.ContainsKey("site") && sceneData["site"].Type == JTokenType.Object)
            {
                result.Item.AddStudio((string)sceneData["site"]["name"]);

                int? site_id = (int)sceneData["site"]["id"], network_id = (int?)sceneData["site"]["network_id"];

                if (network_id.HasValue && !site_id.Equals(network_id))
                {
                    url = Helper.GetSearchSearchURL(siteNum) + $"/sites/{network_id}";

                    var siteData = await GetDataFromApi(url, cancellationToken).ConfigureAwait(false);
                    if (siteData != null)
                    {
                        result.Item.AddStudio((string)siteData["data"]["name"]);
                    }
                }
            }

            var sceneDate = (string)sceneData["date"];
            if (DateTime.TryParseExact(sceneDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
            {
                result.Item.PremiereDate = sceneDateObj;
                var siteName = result.Item.Studios.FirstOrDefault().Replace(" ", string.Empty).ToLower();
                var resultDate = result.Item.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                result.Item.OriginalTitle = $"{siteName} - {resultDate} - {result.Item.Name}";
            }

            if (sceneData.ContainsKey("tags"))
            {
                foreach (var genreLink in sceneData["tags"])
                {
                    var genreName = (string)genreLink["name"];

                    result.Item.AddGenre(genreName);
                }
            }

            if (sceneData.ContainsKey("performers"))
            {
                foreach (var actorLink in sceneData["performers"])
                {
                    var actor = new PersonInfo { Name = (string)actorLink["name"], ImageUrl = (string)actorLink["image"] };

                    result.People.Add(actor);
                }
            }

            return result;
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(
            int[] siteNum,
            string[] sceneID,
            BaseItem item,
            CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (sceneID == null)
            {
                return result;
            }

            var url = Helper.GetSearchSearchURL(siteNum) + $"/scenes/{sceneID[0]}";
            var sceneData = await GetDataFromApi(url, cancellationToken).ConfigureAwait(false);
            if (sceneData == null)
            {
                return result;
            }

            sceneData = (JObject)sceneData["data"];

            result.Add(new RemoteImageInfo { Url = (string)sceneData["poster"], Type = ImageType.Primary });
            result.Add(new RemoteImageInfo { Url = (string)sceneData["background"]["full"], Type = ImageType.Primary });
            result.Add(new RemoteImageInfo { Url = (string)sceneData["background"]["full"], Type = ImageType.Backdrop });

            return result;
        }

        public static async Task<JObject> GetDataFromApi(string url, CancellationToken cancellationToken)
        {
            JObject json = null;
            var headers = new Dictionary<string, string>();
            var token = Environment.GetEnvironmentVariable("API_TOKEN");

            if (!string.IsNullOrEmpty(token))
            {
                headers.Add("Authorization", $"Bearer {token}");
            }
            else if (!string.IsNullOrEmpty(Plugin.Instance.Configuration.PornDbApiToken))
            {
                headers.Add("Authorization", $"Bearer {Plugin.Instance.Configuration.PornDbApiToken}");
            }

            var http = await HTTP.Request(url, cancellationToken, headers).ConfigureAwait(false);
            if (http.IsOK)
            {
                json = JObject.Parse(http.Content);
            }

            return json;
        }
    }
}
