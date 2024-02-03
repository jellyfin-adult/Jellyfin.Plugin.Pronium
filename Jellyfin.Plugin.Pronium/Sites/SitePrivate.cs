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
    public class SitePrivate : IProviderBase
    {
        public async Task<List<RemoteSearchResult>> Search(
            int[] siteNum,
            string searchTitle,
            DateTime? searchDate,
            CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            if (siteNum == null || string.IsNullOrEmpty(searchTitle))
            {
                return results;
            }

            var url = Helper.GetSearchSearchURL(siteNum) + searchTitle;
            Logger.Info($"SitePrivate.Search url: {url}");
            var data = await HTML.ElementFromURL(url, cancellationToken).ConfigureAwait(false);

            var searchResults = data.SelectNodesSafe("//li[@class='card']");
            Logger.Info($"SitePrivate.Search searchResults: {searchResults.Count}");
            foreach (var searchResult in searchResults)
            {
                var sceneLink = searchResult.SelectSingleNode(".//h3/a");
                var sceneUrl = new Uri(sceneLink.Attributes["href"].Value);
                var uniqueId = Helper.Encode(sceneUrl.AbsoluteUri);
                var sceneName = sceneLink.InnerText;
                var scenePoster = searchResult.SelectSingleText(".//img/@src");
                var sceneDate = searchResult.SelectSingleText(".//span[@class='scene-date' or @itemprop='datePublished']");

                var result = new RemoteSearchResult { Name = sceneName, ImageUrl = scenePoster };

                Logger.Info(
                    $"SitePrivate.Search sceneURL: {sceneUrl}, ID: {uniqueId}, sceneName: {sceneName}, scenePoster: {scenePoster}, sceneDate: {sceneDate}");

                if (DateTime.TryParse(sceneDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var sceneDateObj))
                {
                    result.PremiereDate = sceneDateObj;
                }

                Logger.Info($"SitePrivate.Search date: {result.PremiereDate.ToString()}");

                if (result.PremiereDate.HasValue)
                {
                    uniqueId += $"#{result.PremiereDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
                }

                result.ProviderIds.Add(Plugin.Instance?.Name ?? "Pronium", uniqueId);

                if (searchDate == null || result.PremiereDate == null || searchDate.Equals(result.PremiereDate))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        public async Task<MetadataResult<BaseItem>> Update(int[] siteNum, string[] sceneID, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<BaseItem> { Item = new Movie(), People = new List<PersonInfo>() };

            if (sceneID == null)
            {
                return result;
            }

            var sceneUrl = Helper.Decode(sceneID[0]);
            var sceneDate = string.Empty;

            Logger.Info($"SitePrivate.Update sceneID: {sceneID[0]}, sceneURL: {sceneUrl}");

            if (sceneID.Length > 1)
            {
                sceneDate = sceneID[1];
            }

            var sceneData = await HTML.ElementFromURL(sceneUrl, cancellationToken).ConfigureAwait(false);
            var name = sceneData.SelectSingleText("//h1");
            var overview = sceneData.SelectSingleText("//p[@class='sinopsys' or @id='description-section']");

            Logger.Info($"SitePrivate.Update name: {name}, sceneDate: {sceneDate}");
            result.Item.ExternalId = sceneUrl;

            result.Item.Name = name;
            result.Item.Overview = overview
                .Replace("&nbsp;", " ")
                .Trim();

            result.Item.AddStudio("Private");
            var studios = sceneData.SelectNodesSafe("//span[@class='title-site']");
            foreach (var studio in studios)
            {
                var studioName = studio.InnerText;

                if (!string.IsNullOrEmpty(studioName))
                {
                    result.Item.AddStudio(studioName);
                }
            }

            var dvdLine = sceneData.SelectSingleNode("//p[@class='line-dvd']");
            if (dvdLine != null)
            {
                result.Item.AddStudio(dvdLine.InnerHtml);
            }

            if (!string.IsNullOrEmpty(sceneDate))
            {
                if (DateTime.TryParseExact(
                        sceneDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var sceneDateObj))
                {
                    result.Item.PremiereDate = sceneDateObj;
                }
            }
            else
            {
                var date = sceneData.SelectSingleText("//em[contains(., 'Release date:')]/parent::p/span");
                if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(
                        date,
                        "MMMM dd, yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var sceneDateObj))
                {
                    result.Item.PremiereDate = sceneDateObj;
                }
            }

            var genreNode = sceneData.SelectNodesSafe("//li[@class='tag-tags']");
            foreach (var genreLink in genreNode)
            {
                var genreName = genreLink.InnerText;

                result.Item.AddGenre(genreName);
            }

            var actorsNode = sceneData.SelectNodesSafe("//li[@class='tag-models']/a | //span[@itemprop='actor']/a");
            foreach (var actorLink in actorsNode)
            {
                var actor = new PersonInfo { Name = actorLink.InnerText };
                var actorData = await HTML.ElementFromURL(actorLink.Attributes["href"].Value, cancellationToken).ConfigureAwait(false);
                var actorImage = actorData.SelectSingleNode("//a[contains(@class, 'picture-pornstar')]/img");

                if (actorImage != null)
                {
                    actor.ImageUrl = actorImage.Attributes["src"].Value;
                }

                result.People.Add(actor);
            }

            Logger.Info($"SitePrivate.Update genres: {result.Item.Genres.Length}, actors: {result.People.Count}");

            return result;
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(
            int[] siteNum,
            string[] sceneId,
            BaseItem item,
            CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (sceneId == null)
            {
                return result;
            }

            var sceneUrl = Helper.Decode(sceneId[0]);
            var sceneData = await HTML.ElementFromURL(sceneUrl, cancellationToken).ConfigureAwait(false);
            var primaryImage =
                sceneData.SelectSingleNode("//div[@id='trailer_player_finished']//img | //div[contains(@class, dvds-photo)]//img");

            if (primaryImage != null)
            {
                var img = primaryImage.Attributes["src"].Value;

                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Primary });
                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop });
            }

            var id = sceneUrl.Split("/").Last();
            var galleryData = await HTML
                .ElementFromURL($"https://www.private.com/gallery.php?type=highres&id={id}&langx=en", cancellationToken)
                .ConfigureAwait(false);
            var gallery = galleryData.SelectNodesSafe("//a");

            foreach (var photo in gallery)
            {
                var img = photo.Attributes["href"].Value;

                result.Add(new RemoteImageInfo { Url = img, Type = ImageType.Backdrop });
            }

            return result;
        }
    }
}
