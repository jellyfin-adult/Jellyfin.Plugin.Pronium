using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Pronium.Helpers;
using Pronium.Helpers.Utils;

namespace Pronium.Sites
{
    public class ActorFreeones : IProviderBase
    {
        public async Task<List<RemoteSearchResult>> Search(
            int[] siteNum,
            string actorName,
            DateTime? actorDate,
            CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            var url = Helper.GetSearchSearchURL(siteNum) + actorName;
            var actorData = await HTML.ElementFromURL(url, cancellationToken).ConfigureAwait(false);

            foreach (var actorNode in actorData.SelectNodesSafe("//div[contains(@class, 'grid-item')]"))
            {
                var actorURL = new Uri(
                    Helper.GetSearchBaseURL(siteNum) + actorNode.SelectSingleText(".//a/@href")
                        .Replace("/feed", "/bio", StringComparison.OrdinalIgnoreCase));
                string curID = Helper.Encode(actorURL.AbsolutePath),
                    name = actorNode.SelectSingleText(".//p/@title"),
                    imageURL = actorNode.SelectSingleText(".//img/@src");

                var res = new RemoteSearchResult
                {
                    ProviderIds = { { Plugin.Instance?.Name ?? "Pronium", curID } }, Name = name, ImageUrl = imageURL,
                };

                result.Add(res);
            }

            return result;
        }

        public async Task<MetadataResult<BaseItem>> Update(int[] siteNum, string[] sceneId, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<BaseItem> { Item = new Person() };

            var actorUrl = Helper.Decode(sceneId[0]);
            if (!actorUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                actorUrl = Helper.GetSearchBaseURL(siteNum) + actorUrl;
            }

            var actorData = await HTML.ElementFromURL(actorUrl, cancellationToken).ConfigureAwait(false);

            result.Item.ExternalId = actorUrl;

            var name = actorData.SelectSingleText("//span[@data-test='link_span_name']");
            var aliases = string.Join(", ", actorData.SelectNodesSafe("//span[@data-test='link_span_aliases']").Select(t => t.InnerText));

            result.Item.Name = name;
            result.Item.OriginalTitle = name + ", " + aliases;
            result.Item.Overview = actorData.SelectSingleText("//div[@data-test='biography']");

            var actorDate = actorData.SelectSingleText("//span[@data-test='link_span_dateOfBirth']").Trim();

            if (DateTime.TryParseExact(actorDate, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
            {
                result.Item.PremiereDate = sceneDateObj;
            }

            var bornPlaceList = new List<string>();
            var bornPlaceNode = actorData.SelectNodesSafe("//a[@data-test='link_placeOfBirth']");

            foreach (var bornPlace in bornPlaceNode)
            {
                var location = bornPlace.InnerText.Trim();

                if (!string.IsNullOrEmpty(location))
                {
                    bornPlaceList.Add(location);
                }
            }

            result.Item.ProductionLocations = new[] { string.Join(", ", bornPlaceList) };

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

            var actorUrl = Helper.Decode(sceneID[0]);
            if (!actorUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                actorUrl = Helper.GetSearchBaseURL(siteNum) + actorUrl;
            }

            var actorData = await HTML.ElementFromURL(actorUrl, cancellationToken).ConfigureAwait(false);

            var img = actorData.SelectSingleText("//img[contains(@class, 'img-fluid')]/@src");
            if (!string.IsNullOrEmpty(img))
            {
                result.Add(new RemoteImageInfo { Type = ImageType.Primary, Url = img });
            }

            return result;
        }
    }
}
