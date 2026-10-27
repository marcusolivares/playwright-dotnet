namespace Blueprint.Tests.Api;

[Category(TestCategories.Api)]
public sealed class ProductsApiTests : ApiTestBase
{
    [Test]
    public async Task Search_returns_only_products_whose_name_matches()
    {
        const string term = "pliers";

        var response = await Api.Products.SearchAsync(term);

        Assert.That(response.StatusCode, Is.EqualTo(200), response.Content);
        var names = response.Data.Items.Select(product => product.Name).ToList();
        Assert.That(names, Is.Not.Empty);
        Assert.That(names, Has.All.Contains(term).IgnoreCase);
    }
}
