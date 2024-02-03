using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class SiteNaughtyAmericaTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly SiteNaughtyAmerica _site = new();
    private readonly string _testSceneUrl = "https://www.naughtyamerica.com/scene/mgbf-alisondanny-31736";

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(
            new[] { 10, 0 },
            "Busty brunette Alison Rey takes friend's boyfriend's dick for a ride",
            null,
            new CancellationToken());
        Warn.Unless(result.Count, Is.GreaterThan(0));
        var id = result.FirstOrDefault()?.ProviderIds.Values.FirstOrDefault();
        Warn.Unless(id, Is.Not.Empty);
        var url = !string.IsNullOrWhiteSpace(id) ? Helper.Decode(id?.Split('#')[0]) : string.Empty;
        Warn.Unless(url, Is.EqualTo(_testSceneUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 10, 0 }, new[] { Helper.Encode(_testSceneUrl), "2023-03-02" }, new CancellationToken());
        Warn.Unless(result.Item.Name, Is.EqualTo("Busty brunette Alison Rey takes friend's boyfriend's dick for a ride"));
        Warn.Unless(result.Item.Overview, Does.StartWith("Alison Rey has her friend"));
        Warn.Unless(result.Item.Studios.Length, Is.EqualTo(2));
        Warn.Unless(result.Item.Genres.Length, Is.EqualTo(15));
        Warn.Unless(result.People.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 51, 0 },
            new[] { Helper.Encode(_testSceneUrl), "2023-03-02" },
            null,
            new CancellationToken())).ToList();

        Warn.Unless(result, Has.Count.EqualTo(18));
        Warn.Unless(
            result.FirstOrDefault()?.Url,
            Is.EqualTo("https://images1.naughtycdn.com/cms/nacmscontent/v1/scenes/mgbf/alisondanny/scene/horizontal/1279x852c.webp"));
    }
}
