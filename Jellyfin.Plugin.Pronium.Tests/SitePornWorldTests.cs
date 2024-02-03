using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class SitePornWorldTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly SitePornWorld _site = new();
    private readonly string _testSceneByIdUrl = "https://pornworld.com/watch/279717";

    private readonly string _testSceneRegularUrl =
        "https://pornworld.com/watch/465350/insatiable_babe_rebecca_volpetti_gets_dp_d_during_boxing_training_gp2727";

    [Test]
    public async Task RegularSearchIsWorking()
    {
        var result = await _site.Search(
            new[] { 52, 0 },
            "Insatiable Babe Rebecca Volpetti Gets DP'D During Boxing Training",
            null,
            new CancellationToken());
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(21));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneRegularUrl));
    }

    [Test]
    public async Task SearchByIdIsWorking()
    {
        var result = await _site.Search(
            new[] { 52, 0 },
            "Valentine's Day Threesome With Lesbo Lover Shalina Devine and Rebecca Volpetti GP2582",
            null,
            new CancellationToken());
        Assert.That(result.Count, Is.EqualTo(1));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(result[0].Name, Is.EqualTo("Valentine's Day Threesome With Lesbo Lover Shalina Devine and Rebecca Volpetti GP2582"));
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneByIdUrl));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(
            new[] { 52, 0 },
            new[] { Helper.Encode(_testSceneRegularUrl), "2010-09-14" },
            new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Insatiable Babe Rebecca Volpetti Gets DP'd During Boxing Training GP2727"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(1), string.Join(',', result.Item.Studios));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(39), string.Join(',', result.Item.Genres));
        Assert.That(result.Item.Genres, Does.Contain("lingerie"));
        Assert.That(result.Item.Genres, Does.Contain("natural tits"));
        Assert.That(result.People.Count, Is.EqualTo(3), string.Join(',', result.People));
        Assert.That(
            result.People.First().ImageUrl,
            Does.StartWith(
                "https://cdn77-image.gtflixtv.com/unjVaxKNuI_KlS4Mx5fR_w==,1989961200/a372fcf857d2417fb0d4551c42ac24f9ccadbc12/1/733/230/3/1.jpg"));
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 52, 0 },
            new[] { Helper.Encode(_testSceneRegularUrl), "2010-09-14" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(
            result.First().Url,
            Is.EqualTo(
                "https://cdn77-image.gtflixtv.com/X7LxYKUBfEo8K5Jx46m09w==,1989961200/75effc570606e5162ccb947dcce735dd42051e5b/1/2110/841/3/822.jpg?method=resize&w=1354&height=762"));
    }
}
