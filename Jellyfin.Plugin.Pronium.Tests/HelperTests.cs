using Pronium.Helpers;
using Pronium.Helpers.Utils;

namespace Pronium.UnitTests;

[TestFixture]
public class HelperTests
{
    [SetUp]
    public void Setup()
    {
        Database.LoadAll();
    }

    [Test]
    public async Task PrefixIsExtractedProperly()
    {
        Assert.That(Helper.GetSitePrefix(new[] { 0, 5 }), Is.EqualTo("btaw"));
        Assert.That(Helper.GetSitePrefix(new[] { 0, 17 }), Is.EqualTo("ham"));
        Assert.That(Helper.GetSitePrefix(new[] { 0, 16 }), Is.EqualTo("bex"));
        Assert.That(Helper.GetSitePrefix(new[] { 51, 0 }), Is.EqualTo("data18"));
        Assert.That(Helper.GetSitePrefix(new[] { 24, 0 }), Is.EqualTo("teamskeet"));
    }
}
