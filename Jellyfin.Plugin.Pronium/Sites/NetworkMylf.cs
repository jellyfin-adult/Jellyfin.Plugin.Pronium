using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class NetworkMylf : IProviderBase
    {
        private static readonly Dictionary<string, string[]> Genres = new Dictionary<string, string[]>
        {
            { "Anal Mom", new[] { "Anal" } },
            { "BFFs", new[] { "Teen", "Group Sex" } },
            { "Black Valley Girls", new[] { "Teen", "Ebony" } },
            { "DadCrush", new[] { "Step Dad", "Step Daughter" } },
            { "DaughterSwap", new[] { "Step Dad", "Step Daughter" } },
            { "Dyked", new[] { "Hardcore", "Teen", "Lesbian" } },
            { "Exxxtra Small", new[] { "Teen", "Small Tits" } },
            { "Family Strokes", new[] { "Taboo Family" } },
            { "Foster Tapes", new[] { "Taboo Sex" } },
            { "Full Of JOI", new[] { "JOI" } },
            { "Little Asians", new[] { "Teen", "Asian" } },
            { "Lone Milf", new[] { "Solo" } },
            { "Milf Body", new[] { "Gym", "Fitness" } },
            { "Milfty", new[] { "Cheating" } },
            { "Mom Drips", new[] { "Creampie" } },
            { "MylfBlows", new[] { "Blowjob" } },
            { "MylfBoss", new[] { "Office", "Boss" } },
            { "MylfDom", new[] { "BDSM" } },
            { "Mylfed", new[] { "Lesbian" } },
            { "PervMom", new[] { "Step Mom" } },
            { "Shoplyfter", new[] { "Strip" } },
            { "ShoplyfterMylf", new[] { "Strip", "MILF" } },
            { "Sis Loves Me", new[] { "Step Sister" } },
            { "TeenJoi", new[] { "Teen", "JOI" } },
            { "Teens Love Black Cocks", new[] { "Teen", "BBC" } },
            { "Thickumz", new[] { "Thick" } },
        };

        public async Task<List<RemoteSearchResult>> Search(
            int[] siteNum,
            string searchTitle,
            DateTime? searchDate,
            CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            var searchUrl = Helper.GetSearchSearchURL(siteNum) + searchTitle;

            Logger.Info($"NetworkMylf.Search url: {searchUrl}");
            var searchData = await GetJsonFromPage(searchUrl, cancellationToken).ConfigureAwait(false);
            var searchResults = searchData["searchResults"]["items"]["pages"][0] as JArray;

            Logger.Info($"NetworkMylf.Search searchResults: {searchResults.Count}");

            foreach (var searchResult in searchResults)
            {
                var result = new RemoteSearchResult
                {
                    Name = searchResult["title"].ToString(),
                    ImageUrl = searchResult["img"].ToString(),
                    PremiereDate = DateTime.Parse(searchResult["publishedDate"].ToString()),
                };
                var sceneId = Helper.Encode($"{Helper.GetSearchBaseURL(siteNum)}/movies/{searchResult["id"]}");

                if (result.PremiereDate.HasValue)
                {
                    sceneId += $"#{result.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
                }

                result.ProviderIds.Add(Plugin.Instance?.Name ?? "Pronium", sceneId);

                results.Add(result);
            }

            return results;
        }

        public async Task<MetadataResult<BaseItem>> Update(int[] siteNum, string[] sceneID, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<BaseItem> { Item = new Movie(), People = new List<PersonInfo>() };

            if (sceneID == null || siteNum == null)
            {
                return result;
            }

            string sceneURL = Helper.Decode(sceneID[0]), sceneDate = string.Empty;

            if (!sceneURL.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                sceneURL = Helper.GetSearchBaseURL(siteNum) + sceneURL;
            }

            if (sceneID.Length > 1)
            {
                sceneDate = sceneID[1];
            }

            var sceneData = await GetJsonFromPage(sceneURL, cancellationToken).ConfigureAwait(false);
            if (sceneData == null)
            {
                return result;
            }

            var contentName = string.Empty;
            foreach (var name in new List<string> { "moviesContent", "videosContent" })
            {
                if (sceneData.ContainsKey(name) && sceneData[name].Any())
                {
                    contentName = name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(contentName))
            {
                return result;
            }

            sceneData = (JObject)sceneData[contentName];
            var sceneName = sceneData.Properties().First().Name;
            sceneData = (JObject)sceneData[sceneName];

            result.Item.ExternalId = sceneURL;

            result.Item.Name = (string)sceneData["title"];
            result.Item.Overview = sceneData["description"].ToString().Replace("<p>", string.Empty).Replace("</p>", string.Empty);
            switch (siteNum[0])
            {
                case 23:
                    result.Item.AddStudio("Mylf");
                    break;

                case 24:
                    result.Item.AddStudio("TeamSkeet");
                    break;
            }

            result.Item.AddStudio(sceneData["site"]["name"].ToString());

            DateTime? releaseDate = null;
            if (sceneData.ContainsKey("publishedDate"))
            {
                releaseDate = (DateTime)sceneData["publishedDate"];
            }
            else
            {
                if (DateTime.TryParseExact(
                        sceneDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var sceneDateObj))
                {
                    releaseDate = sceneDateObj;
                }
            }

            if (releaseDate.HasValue)
            {
                result.Item.PremiereDate = releaseDate.Value;
            }

            string subSite;
            if (sceneData.ContainsKey("site"))
            {
                subSite = (string)sceneData["site"]["name"];
            }
            else
            {
                subSite = Helper.GetSearchSiteName(siteNum);
            }

            if (Genres.ContainsKey(subSite))
            {
                foreach (var genreName in Genres[subSite])
                {
                    result.Item.AddGenre(genreName);
                }
            }

            foreach (var genreName in sceneData["tags"])
            {
                result.Item.AddGenre(genreName.ToString());
            }

            foreach (var actorLink in sceneData["models"])
            {
                string actorName = (string)actorLink["modelName"], actorID = (string)actorLink["modelId"], actorPhotoURL;

                var actorData = await GetJsonFromPage($"{Helper.GetSearchBaseURL(siteNum)}/models/{actorID}", cancellationToken)
                    .ConfigureAwait(false);
                if (actorData != null)
                {
                    actorPhotoURL = (string)actorData["modelsContent"][actorID]["img"];
                    result.People.Add(new PersonInfo { Name = actorName, ImageUrl = actorPhotoURL });
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

            var sceneURL = Helper.Decode(sceneID[0]);
            if (!sceneURL.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                sceneURL = Helper.GetSearchBaseURL(siteNum) + sceneURL;
            }

            var sceneData = await GetJsonFromPage(sceneURL, cancellationToken).ConfigureAwait(false);
            if (sceneData == null)
            {
                return result;
            }

            var contentName = string.Empty;
            foreach (var name in new List<string> { "moviesContent", "videosContent" })
            {
                if (sceneData.ContainsKey(name) && sceneData[name] != null)
                {
                    contentName = name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(contentName))
            {
                return result;
            }

            sceneData = (JObject)sceneData[contentName];
            var sceneName = sceneData.Properties().First().Name;
            sceneData = (JObject)sceneData[sceneName];

            var img = (string)sceneData["img"];
            result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Primary });
            result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop });

            return result;
        }

        private async Task<JObject> GetJsonFromPage(string url, CancellationToken cancellationToken)
        {
            JObject json = null;

            var http = await HTTP.Request(url, cancellationToken).ConfigureAwait(false);
            if (http.IsOK)
            {
                var regEx = new Regex(@"window\.__INITIAL_STATE__ = (.*);").Match(http.Content);
                if (regEx.Groups.Count > 0)
                {
                    json = (JObject)JObject.Parse(regEx.Groups[1].Value)["content"];
                }
            }

            return json;
        }
    }
}
