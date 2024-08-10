using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class SiteClips4SaleTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly SiteClips4Sale _site = new();

    private readonly string _testSceneUrl = "/studio/33729/24251761/mf-horny-step-son-fucks-his-step-mom-hd-taboo-mandy-flores";

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(
            new[] { 49, 0 },
            "33729 Horny Step-Son fucks his Step-Mom",
            DateTime.Parse("2020-12-12"),
            new CancellationToken());
        Assert.That(result.Count, Is.GreaterThan(0));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 49, 0 }, new[] { Helper.Encode(_testSceneUrl), "2020-12-12" }, new CancellationToken());
        Assert.That(result.Item.Name, Does.Contain("Horny Step-Son fucks his Step-Mom"));
        Assert.That(result.Item.OriginalTitle, Is.EqualTo("c4s - 33729 MF~ Horny Step-Son fucks his Step-Mom : Taboo: Mandy Flores"));
        Assert.That(result.Item.Overview, Is.Not.Empty);
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(6));
        Assert.That(result.People.Count, Is.EqualTo(0));
#pragma warning disable NUnit2021 // Incompatible types for EqualTo constraint
        Assert.That(result.Item.PremiereDate, Is.EqualTo(DateTime.Parse("2020-12-12")));
#pragma warning restore NUnit2021 // Incompatible types for EqualTo constraint
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 49, 0 },
            new[] { Helper.Encode(_testSceneUrl), "2020-12-12" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First().Url, Is.EqualTo("https://imagecdn.clips4sale.com/accounts99/33729/clip_images/previewlg_24251761.jpg"));
    }
}
