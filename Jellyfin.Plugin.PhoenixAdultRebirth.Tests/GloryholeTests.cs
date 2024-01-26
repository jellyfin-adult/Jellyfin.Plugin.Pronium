using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class GloryholeTests
{
    private readonly NetworkGammaEnt _site = new();
    private readonly string _testSceneUrl = "https://www.gloryhole.com/en/video/gloryhole/Alena-Croft/221961";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new []{19, 3}, "Alena Croft", DateTime.Parse("2015-09-28"), new CancellationToken());
        Assert.That(result.Count, Is.EqualTo(2));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.IsNotEmpty(id);
        Assert.That(id?.Split('#')[0], Is.EqualTo("221961"));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new []{19, 3}, new []{"221961", "scenes", "2015-09-28"}, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Alena Croft"));
        Assert.IsNotEmpty(result.Item.Overview);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(6));
        Assert.That(result.People.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = await _site.GetImages(new[] { 19, 3 }, new []{"221961", "scenes", "2015-09-28"}, null, new CancellationToken());

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Url, Is.EqualTo("https://images-fame.gammacdn.com/movies/104458/104458_01/previews/2/685/top_1_resized/104458_01_01.jpg"));
    }
}
