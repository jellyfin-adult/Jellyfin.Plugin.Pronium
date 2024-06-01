using Pronium.Helpers.Utils;
using Pronium.Providers;

namespace Pronium.UnitTests;

[TestFixture]
public class MovieProviderTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    private readonly MovieProvider _provider = new();

    [Test]
    public async Task SearchIsWorking()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "slayed - dollhouse" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(3));
    }

    [Test]
    public async Task SearchForMoviesIsWorking()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "Dark Woods" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(30));
    }

    [Test]
    public async Task SearchForUniqueMovie()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "Confidential file" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(5));
        Assert.That(data.Any(t => t.ProviderIds.Values.First().Contains("48#1#2335556")), Is.True);
    }
}
