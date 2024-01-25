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
        Assert.AreEqual(TestSceneUrl, Helpers.Helper.Decode(id.Split('#')[0]));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await Site.Update(new []{51, 0}, new []{Helpers.Helper.Encode(TestSceneUrl), "2010-09-14"}, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Elite Dyke Society"));
        Assert.IsNotEmpty(result.Item.Overview);
        Assert.AreEqual(3, result.Item.Studios.Length);
        Assert.AreEqual(6, result.Item.Genres.Length);
        Assert.AreEqual(2, result.People.Count);
    }
}
