using PhoenixAdultRebirth.Helpers;
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
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 22, 0 }, new[] { Helper.Encode(_testSceneUrl), "2023-02-23" }, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("2023-02-23"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(3));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(6));
        Assert.That(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 22, 0 },
            new[] { Helper.Encode(_testSceneUrl), "2023-02-23" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(
            result.First().Url,
            Is.EqualTo("https://cdn.dt18.com/media/t/1/scenes/1/2/44118-kelly-divine-sara-jaymes-hot-and-mean.jpg"));
    }
}
