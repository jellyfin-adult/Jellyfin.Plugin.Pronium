using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;

namespace Pronium.UnitTests;

[TestFixture]
public class SiteFreeUseFantasyTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly NetworkMylf _site = new();
    private readonly string _testSceneUrl = "https://www.teamskeet.com/movies/show-us-what-youre-good-at";

    [Test]
    public async Task SearchIsWorking()
    {
        var result = await _site.Search(new[] { 24, 58 }, "Show Us What You’re Good at", null, new CancellationToken());
        Assert.That(result.Count, Is.GreaterThan(10));
        var id = result[0].ProviderIds.Values.FirstOrDefault();
        Assert.That(id, Is.Not.Empty);
        Assert.That(Helper.Decode(id?.Split('#')[0]), Is.EqualTo(_testSceneUrl));
        Assert.That(result.First().ImageUrl, Does.Contain("daisy_stone_and_dharma_jones"));
    }

    [Test]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 24, 58 }, new[] { Helper.Encode(_testSceneUrl), "2024-02-03" }, new CancellationToken());
        Assert.That(result.Item.Name, Is.EqualTo("Show Us What You’re Good at"));
        Assert.That(result.Item.OriginalTitle, Is.EqualTo("freeusefantasy - 2024-02-03 - Show Us What You’re Good at"));
        Assert.That(result.Item.Overview, Does.StartWith("It’s been a while since Daisy is jobless, and her roommate"));
        Assert.That(result.Item.Studios.Length, Is.EqualTo(2));
        Assert.That(result.Item.Genres.Length, Is.EqualTo(82));
        Assert.That(result.Item.Genres, Does.Contain("Barefoot"));
        Assert.That(result.People.Count, Is.EqualTo(2));
        Assert.That(result.People.Select(t => t.Name), Does.Contain("Daisy Stone"));
        Assert.That(result.People.Select(t => t.Name), Does.Contain("Dharma Jones"));
#pragma warning disable NUnit2021 // Incompatible types for EqualTo constraint
        Assert.That(result.Item.PremiereDate, Is.EqualTo(new DateTime(2024, 02, 03)).Within(24).Hours);
#pragma warning restore NUnit2021 // Incompatible types for EqualTo constraint
    }

    [Test]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 24, 58 },
            new[] { Helper.Encode(_testSceneUrl), "2024-02-03" },
            null,
            new CancellationToken())).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First().Url, Does.Contain("daisy_stone_and_dharma_jones"));
    }
}
