using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class PornDbApiTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly PornDbApi _site = new();

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new[] { 48, 0 }, "Anal Maid Service", null, new CancellationToken());
        Assert.That(result.Count, Is.GreaterThan(0));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(id, Is.EqualTo("48#0#2751895"));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 48, 0 }, new[] { "2751895" }, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Anal Maid Service"));
        Assert.That(result.Item.OriginalTitle, Is.EqualTo("mydirtymaid - 2023-02-23 - Anal Maid Service"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(15));
        Assert.That(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(new[] { 48, 0 }, new[] { "2751895" }, null, new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.First().Url, Does.Contain("my-dirty-maid-anal-maid"));
    }
}
