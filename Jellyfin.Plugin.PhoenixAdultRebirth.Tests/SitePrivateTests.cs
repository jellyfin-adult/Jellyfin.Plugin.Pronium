using System.Globalization;
using PhoenixAdultRebirth.Helpers;
using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class SitePrivateTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly SitePrivate _site = new();
    private readonly string _testMovieUrl = "https://www.private.com/movie/731-the-bachelorette-bang/";

    private readonly string _testSceneUrl =
        "https://www.private.com/scene/blonde-babe-kristi-lust-wears-bikini-on-boat-before-hardcore-threeway/9281";

    [Test]
    public async Task SearchForMovieIsWorking()
    {
        var result = await _site.Search(new[] { 53, 0 }, "The Bachelorette Bang",
            DateTime.Parse("10/01/2011", CultureInfo.InvariantCulture), new CancellationToken());
        Assert.That(result.Count, Is.EqualTo(1));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testMovieUrl));
    }

    [Test]
    public async Task SearchForSceneIsWorking()
    {
        var result = await _site.Search(new[] { 53, 0 },
            "Blonde Babe Kristi Lust Wears Bikini on Boat before Hardcore Threeway",
            DateTime.Parse("05/06/2012", CultureInfo.InvariantCulture), new CancellationToken());
        Assert.That(result.Count, Is.EqualTo(1));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateSceneIsWorking()
    {
        var result = await _site.Update(new[] { 53, 0 }, new[] { Helper.Encode(_testSceneUrl), "2012-05-06" },
            new CancellationToken());
        Assert.That(result.Item.Name,
            Is.EqualTo("Blonde Babe Kristi Lust Wears Bikini on Boat before Hardcore Threeway"));
        Assert.That(result.Item.Overview, Does.StartWith("Kristy Lust gets a little bit grumpy "));
        Assert.That(result.Item.Studios.Length, Is.EqualTo(1));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(1));
        Assert.That(result.People.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task UpdateMovieIsWorking()
    {
        var result = await _site.Update(new[] { 53, 0 }, new[] { Helper.Encode(_testMovieUrl), "2011-10-01" },
            new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("The Bachelorette Bang"));
        Assert.That(result.Item.Overview, Does.StartWith("Monica is out for "));
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(0));
        Assert.That(result.People.Count, Is.EqualTo(6));
    }

    [Test]
    public async Task GetImagesForSceneIsWorking()
    {
        var result = (await _site.GetImages(new[] { 53, 0 }, new[] { Helper.Encode(_testSceneUrl), "2010-09-14" }, null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(17));
        Assert.That(result.First().Url, Does.StartWith("https://pcom77.st-content.com/content/contentthumbs/325301.jpg"));
    }

    [Test]
    public async Task GetImagesForMovieIsWorking()
    {
        var result = (await _site.GetImages(new[] { 53, 0 }, new[] { Helper.Encode(_testMovieUrl), "2010-09-14" }, null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First().Url, Does.StartWith("https://pcom77.st-content.com/content/contentthumbs/22236-dvdasc.jpg"));
    }
}
