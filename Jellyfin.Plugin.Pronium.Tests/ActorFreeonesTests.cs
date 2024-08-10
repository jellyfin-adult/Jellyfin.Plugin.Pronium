using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class ActorFreeonesTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly ActorFreeones _site = new();
    private readonly string _testSceneUrl = "/natasha-nice/bio";

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new[] { 43, 0 }, "Natasha Nice", null, new CancellationToken());
        Assert.That(result.Count, Is.EqualTo(1));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
        Assert.That(result.First().ImageUrl, Does.Contain("Natasha-Nice"));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 43, 0 }, new[] { Helper.Encode(_testSceneUrl), "2024-02-17" }, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Natasha Nice"));
        Assert.That(result.Item.OriginalTitle, Does.Contain("Natasha Nyce, Nat Nice"));
        Assert.That(result.Item.Overview, Does.StartWith("One of the best sounds in the world "));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 43, 0 },
            new[] { Helper.Encode(_testSceneUrl), "2024-02-17" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Url, Does.Contain("Natasha-Nice"));
    }
}
