using PhoenixAdultRebirth.Helpers;
using PhoenixAdultRebirth.Helpers.Utils;
using PhoenixAdultRebirth.Sites;

namespace PhoenixAdultRebirth.UnitTests;

public class SiteNaughtyAmericaTests
{
    private readonly SiteNaughtyAmerica _site = new();
    private readonly string _testSceneUrl = "https://www.naughtyamerica.com/scene/mgbf-alisondanny-31736";

    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new[] { 10, 0 }, "Busty brunette Alison Rey takes friend's boyfriend's dick for a ride", null,
            new CancellationToken());
        Warn.If(result.Count, Is.GreaterThan(0));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Warn.If(id, Is.Not.Empty);
        Warn.If(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 10, 0 }, new[] { Helper.Encode(_testSceneUrl), "2023-03-02" }, new CancellationToken());
        Warn.If(result.Item.Name, Is.EqualTo("Busty brunette Alison Rey takes friend's boyfriend's dick for a ride"));
        Warn.If(result.Item.Overview, Is.Not.Empty);
        Warn.If(result.Item.Studios.Length, Is.EqualTo(2));
        Warn.If(result.Item.Genres.Length, Is.EqualTo(15));
        Warn.If(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(new[] { 51, 0 }, new[] { Helper.Encode(_testSceneUrl), "2023-03-02" }, null,
            new CancellationToken())).ToList();

        Warn.If(result, Has.Count.EqualTo(18));
        Warn.If(result.First().Url,
            Is.EqualTo("https://images1.naughtycdn.com/cms/nacmscontent/v1/scenes/mgbf/alisondanny/scene/horizontal/1279x852c.webp"));
    }
}
