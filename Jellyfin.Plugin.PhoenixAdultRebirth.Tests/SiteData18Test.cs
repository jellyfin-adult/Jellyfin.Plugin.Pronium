using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class Tests
{
    private readonly SiteData18 Site = new SiteData18();
    private string TestSceneUrl = "https://www.data18.com/scenes/144118";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await Site.Search(new []{51, 0}, "Elite Dyke Society", null, new CancellationToken());
        Assert.IsTrue(result.Count > 0);
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.IsNotEmpty(id);
        Assert.That(Helpers.Helper.Decode(id?.Split('#')[0]), Is.EqualTo(TestSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await Site.Update(new []{51, 0}, new []{Helpers.Helper.Encode(TestSceneUrl), "2010-09-14"}, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Elite Dyke Society"));
        Assert.IsNotEmpty(result.Item.Overview);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(3));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(6));
        Assert.That(result.People.Count, Is.EqualTo(2));
    }
}
