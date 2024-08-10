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
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchIsWorking()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "slayed - dollhouse" }, new CancellationToken());

        Assert.That(data.Count(), Is.EqualTo(3));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchForMoviesIsWorking()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "Dark Woods" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(30));
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task SearchForUniqueMovie()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "Confidential file" }, new CancellationToken());

        Assert.That(data.Count(), Is.GreaterThan(5));
        Assert.That(data.Any(t => t.ProviderIds.Values.First().Contains("48#1#2335556")), Is.True);
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task FullNameSearchPornworld()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "pornworld - 2016-10-02 - Double Dong Delight - a Huge Glass Dick Does the Trick" }, new CancellationToken());

        Assert.That(data.Any(t => t.ProviderIds.Values.First().Contains("48#0#656865")), Is.True);
    }

    [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task FullNameSearchPrivate()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "private - 2001-04-01 - Eternal Love" }, new CancellationToken());

        Assert.That(data.Any(t => t.ProviderIds.Values.First().Contains("53#0#sML2JBPsdAyS6VtxWpWJUR3t91ABfPBX1z7rC2cQBRZCyXoRPm7Qqi6nVmtxWnTY")), Is.True);
    }

        [Test]
    [TestCase(TestName = "{c}.{m}")]
    public async Task FullNameSearchBrazzersMissingOnOfficial()
    {
        var data = await _provider.GetSearchResults(new MediaBrowser.Controller.Providers.MovieInfo { Name = "rws - 2016-06-08 - One Last Shot" }, new CancellationToken());

        Assert.That(data.Any(t => t.ProviderIds.Values.First().Contains("48#0#799589")), Is.True);
    }
}
