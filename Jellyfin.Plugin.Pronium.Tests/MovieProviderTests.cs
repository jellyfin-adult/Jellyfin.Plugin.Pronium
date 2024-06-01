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

        Assert.That(data.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task SearchForMoviesIsWorking()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "Dark Woods" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(30));
    }
}
