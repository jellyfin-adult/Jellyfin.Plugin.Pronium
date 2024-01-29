using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class SiteBangBrosTests
{
    private readonly Network1service _site = new();
    private readonly string _testSceneUrl = "https://bangbros.com/video/8854561/anal-maid-serviceS";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new[] { 22, 0 }, "Anal Maid Service", null, new CancellationToken());
        Assert.That(result.Count, Is.GreaterThan(0));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(id?.Split('#')[0], Is.EqualTo("8854561"));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 22, 0 }, new[] { "8854561", "scene" }, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Anal Maid Service"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(15));
        Assert.That(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 22, 0 },
            new[] { "8854561", "scene" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(12));
        Assert.That(
            result.First().Url,
            Is.EqualTo(
                "https://media-public-ht.project1content.com/m=ea_aGJcWx/71b/47b/ead/1c2/45b/a89/1d0/879/93a/335/f9/poster/poster_01.jpg"));
    }
}
