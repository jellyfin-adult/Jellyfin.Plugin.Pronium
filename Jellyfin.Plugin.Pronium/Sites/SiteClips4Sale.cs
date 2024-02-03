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
using Pronium.Helpers;
using Pronium.Helpers.Utils;

namespace Pronium.Sites
{
    public class SiteClips4Sale : IProviderBase
    {
        private static readonly IList<string> TitleCleanupWords = new List<string>
        {
            "HD",
            "mp4",
            "wmv",
            "360p",
            "480p",
            "720p",
            "1080p",
            "()",
            "( - )",
        };

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

            var parts = searchTitle.Split(' ');
            if (!int.TryParse(parts[0], out var studioId))
            {
                return result;
            }

            searchTitle = string.Join(" ", parts.Skip(1));

            var domain = Helper.GetSearchSearchURL(siteNum);
            var studioData = await HTML.ElementFromURL($"{domain}{studioId}", cancellationToken).ConfigureAwait(false);
            var studioUrl = studioData.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value;
            var url = $"{studioUrl}/Cat0-AllCategories/Page1/C4SSort-added_at/Limit24/search/{searchTitle}";
            var data = await HTML.ElementFromURL(url, cancellationToken).ConfigureAwait(false);

            var searchResults = data.SelectNodesSafe("//ul[contains(@class, 'w-full')]/li");
            foreach (var searchResult in searchResults)
            {
                var sceneLink = searchResult.SelectSingleNode(".//a[contains(@class, 'search-clip__title')]");
                var sceneURL = new Uri(Helper.GetSearchBaseURL(siteNum) + sceneLink.Attributes["href"].Value);
                var sceneId = GetSceneIdFromSceneURL(sceneURL.AbsoluteUri);
                var curID = Helper.Encode(sceneURL.PathAndQuery);
                var sceneName = CleanupTitle(sceneLink.InnerText);
                var scenePoster = GetPosterUrl(studioId, sceneId);

                var res = new RemoteSearchResult
                {
                    ProviderIds = { { Plugin.Instance?.Name ?? "Pronium", curID } }, Name = sceneName, ImageUrl = scenePoster,
                };

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

            var sceneUrl = Helper.Decode(sceneID[0]);
            if (!sceneUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                sceneUrl = Helper.GetSearchBaseURL(siteNum) + sceneUrl;
            }

            var sceneData = await HTML.ElementFromURL(sceneUrl, cancellationToken).ConfigureAwait(false);

            result.Item.ExternalId = sceneUrl;
            result.Item.Name = CleanupTitle(sceneData.SelectSingleText("//h1"));
            result.Item.Overview = sceneData.SelectSingleText("//div[contains(@class, 'read-more--box')]");

            result.Item.AddStudio("Clips4Sale");
            var studioName = sceneData.SelectSingleText("//div[@id='content']/div[1]/div/div/div[1]/a");
            if (!string.IsNullOrEmpty(studioName))
            {
                result.Item.AddStudio(studioName);
            }

            var sceneDate = sceneData.SelectSingleText("//h1/parent::div/div/div/div/div[1]/span[1]")?.Split(" ").First();
            if (DateTime.TryParseExact(sceneDate, "M/d/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
            {
                result.Item.PremiereDate = sceneDateObj;
            }

            var category = sceneData.SelectSingleText("//span[contains(text(), 'Category')]/parent::div/span[2]/a");
            result.Item.AddGenre(category);
            foreach (var genreLink in sceneData.SelectNodesSafe("//span[contains(text(), 'Related Categories')]/parent::div/span[2]/a"))
            {
                var genreName = genreLink.InnerText;

                result.Item.AddGenre(genreName);
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

            var sceneId = GetSceneIdFromSceneURL(sceneURL);
            var studioId = GetStudioIdFromSceneURL(sceneURL);

            var img = GetPosterUrl(studioId, sceneId);
            if (!string.IsNullOrEmpty(img))
            {
                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Primary });
                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop });
            }

            return result;
        }

        private static int GetSceneIdFromSceneURL(string sceneUrl)
        {
            return int.Parse(sceneUrl.Split("://").Last().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[3]);
        }

        private static int GetStudioIdFromSceneURL(string sceneUrl)
        {
            return int.Parse(sceneUrl.Split("://").Last().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2]);
        }

        private static string GetPosterUrl(int studioId, int sceneId)
        {
            return $"https://imagecdn.clips4sale.com/accounts99/{studioId}/clip_images/previewlg_{sceneId}.jpg";
        }

        private static string CleanupTitle(string title)
        {
            foreach (var word in TitleCleanupWords)
            {
                if (title.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    title = title.Replace(word, string.Empty, StringComparison.OrdinalIgnoreCase);
                }
            }

            title = title.Trim();

            if (title.EndsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                title = title.Remove(title.Length - 1, 1);
            }

            return title;
        }
    }
}
