using Pronium.Helpers;
using Pronium.Helpers.Utils;
using Pronium.Sites;
using MediaBrowser.Model.Providers;

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
    private readonly string _testSceneUrl = "https://www.teamskeet.com/movies/freeused-freeloader";

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchIsWorking()
    {
        var result = new List<RemoteSearchResult>();
        try {
            result = await _site.Search(new[] { 24, 58 }, "Freeused Freeloader", null, new CancellationToken());
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }

        Warn.Unless(result.Count, Is.GreaterThan(1));
        var id = result.FirstOrDefault()?.ProviderIds.Values.FirstOrDefault();
        Warn.Unless(id, Is.Not.Empty);
        Warn.Unless(id != null ? Helper.Decode(id.Split('#')[0]) : "", Is.EqualTo(_testSceneUrl));
        Warn.Unless(result.FirstOrDefault()?.ImageUrl ?? "", Does.Contain("breezy_bri_and_penelope_woods"));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task UpdateIsWorking()
    {
        var result = await _site.Update(new[] { 24, 58 }, new[] { Helper.Encode(_testSceneUrl), "2024-08-10" }, new CancellationToken());

        Warn.Unless(result.Item.Name, Is.EqualTo("Freeused Freeloader"));
        Warn.Unless(result.Item.OriginalTitle, Is.EqualTo("freeusefantasy - 2024-08-10 - Freeused Freeloader"));
        Warn.Unless(result.Item.Overview, Does.StartWith("Tyler is pissed at Penelope. She’s not contributing to rent"));
        Warn.Unless(result.Item.Studios.Length, Is.EqualTo(2));
        Warn.Unless(result.Item.Genres.Length, Is.EqualTo(86));
        Warn.Unless(result.Item.Genres, Does.Contain("Belly Button Piercings"));
        Warn.Unless(result.People.Count, Is.EqualTo(2));
        Warn.Unless(result.People.Select(t => t.Name), Does.Contain("Penelope Woods"));
        Warn.Unless(result.People.Select(t => t.Name), Does.Contain("Breezy Bri"));
#pragma warning disable NUnit2021 // Incompatible types for EqualTo constraint
        Warn.Unless(result.Item.PremiereDate, Is.EqualTo(new DateTime(2024, 08, 10)).Within(24).Hours);
#pragma warning restore NUnit2021 // Incompatible types for EqualTo constraint
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task GetImagesIsWorking()
    {
        var result = (await _site.GetImages(
            new[] { 24, 58 },
            new[] { Helper.Encode(_testSceneUrl), "2024-08-10" },
            null,
            new CancellationToken())).ToList();

        Warn.Unless(result, Has.Count.EqualTo(2));
        Warn.Unless(result.FirstOrDefault()?.Url ?? "", Does.Contain("breezy_bri_and_penelope_woods"));
    }
}
