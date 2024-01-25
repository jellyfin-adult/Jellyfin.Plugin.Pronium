using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class Tests
{
    private readonly SiteData18 _site = new();
    private readonly string _testSceneUrl = "https://www.data18.com/scenes/144118";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new []{51, 0}, "Elite Dyke Society", null, new CancellationToken());
        Assert.IsTrue(result.Count > 0);
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.IsNotEmpty(id);
        Assert.That(Helpers.Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new []{51, 0}, new []{Helpers.Helper.Encode(_testSceneUrl), "2010-09-14"}, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Elite Dyke Society"));
        Assert.IsNotEmpty(result.Item.Overview);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(3));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(6));
        Assert.That(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = await _site.GetImages(new[] { 51, 0 }, new[] { Helpers.Helper.Encode(_testSceneUrl), "2010-09-14" }, null, new CancellationToken());

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Url, Is.EqualTo("https://cdn.dt18.com/media/t/1/scenes/1/2/44118-kelly-divine-sara-jaymes-hot-and-mean.jpg"));
    }
}
