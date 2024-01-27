using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class SitePornWorldTests
{
    private readonly SitePornWorld _site = new();
    private readonly string _testSceneUrl = "https://pornworld.com/watch/465350/insatiable_babe_rebecca_volpetti_gets_dp_d_during_boxing_training_gp2727";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new []{52, 0}, "Insatiable Babe Rebecca Volpetti Gets DP'D During Boxing Training GP2727", null, new CancellationToken());
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(21));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helpers.Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new []{52, 0}, new []{Helpers.Helper.Encode(_testSceneUrl), "2010-09-14"}, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Insatiable Babe Rebecca Volpetti Gets DP'd During Boxing Training GP2727"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(1));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(40));
        Assert.That(result.People.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(new[] { 51, 0 }, new[] { Helpers.Helper.Encode(_testSceneUrl), "2010-09-14" }, null, new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First().Url, Is.EqualTo("https://cdn77-image.gtflixtv.com/X7LxYKUBfEo8K5Jx46m09w==,1989961200/75effc570606e5162ccb947dcce735dd42051e5b/1/2110/841/3/822.jpg?method=resize&w=1354&height=762"));
    }
}
