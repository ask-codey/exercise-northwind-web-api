using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NorthwindApi.Data;
using NorthwindApi.Errors;
using NorthwindApi.Models;

namespace NorthwindApi.Tests;

// These tests start your whole API in memory and send it requests, the way a client would.
// They swap the real ProductDao for a FakeProductDao, so they need no database.
// You don't need to change this file.
[TestClass]
public class ProductsApiTests
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private FakeProductDao fake = null!;

    [TestInitialize]
    public void StartTheApi()
    {
        fake = new FakeProductDao();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => services.AddScoped<IProductDao>(_ => fake));
        });
        client = factory.CreateClient();
    }

    [TestCleanup]
    public void StopTheApi()
    {
        client.Dispose();
        factory.Dispose();
    }

    // A 404 from NotFound() comes with a problem details body; a route that doesn't exist has none.
    private static void AssertNotFoundFromTheAction(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType,
            "The 404 should come from the action's NotFound(), not from a missing route.");
    }

    private static Product NewProduct(string name) =>
        new Product { ProductName = name, CategoryId = 3, UnitPrice = 4.50m, UnitsInStock = 20 };

    [TestMethod]
    public async Task GetAll_NoFilter_AnswersEveryProduct()
    {
        HttpResponseMessage response = await client.GetAsync("/api/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        List<Product>? products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.HasCount(3, products!);
    }

    [TestMethod]
    public async Task GetAll_WithCategoryId_AnswersOnlyThatCategory()
    {
        HttpResponseMessage response = await client.GetAsync("/api/products?categoryId=1");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        List<Product>? products = await response.Content.ReadFromJsonAsync<List<Product>>();
        CollectionAssert.AreEqual(new[] { "Chai", "Chang" }, products!.Select(p => p.ProductName).ToArray());
    }

    [TestMethod]
    public async Task GetById_Existing_AnswersTheProduct()
    {
        HttpResponseMessage response = await client.GetAsync("/api/products/3");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Product? product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.AreEqual("Aniseed Syrup", product!.ProductName);
    }

    [TestMethod]
    public async Task GetById_Missing_Answers404()
    {
        HttpResponseMessage response = await client.GetAsync("/api/products/999");

        AssertNotFoundFromTheAction(response);
    }

    [TestMethod]
    public async Task Add_ValidProduct_Answers201WithTheNewProductAndItsAddress()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", NewProduct("Trail Mix"));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Product? added = await response.Content.ReadFromJsonAsync<Product>();
        Assert.AreEqual(4, added!.ProductId);
        Assert.AreEqual("Trail Mix", added.ProductName);
        Assert.AreEqual("/api/products/4", response.Headers.Location?.AbsolutePath);
        Assert.IsTrue(fake.Products.Any(p => p.ProductName == "Trail Mix"), "The product should be saved.");
    }

    [TestMethod]
    public async Task Add_NoName_Answers400AndSavesNothing()
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/products", new { unitPrice = 4.50 });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.HasCount(3, fake.Products);
    }

    [TestMethod]
    public async Task Update_Existing_Answers204AndSavesTheChange()
    {
        Product changed = NewProduct("Chai Latte");

        HttpResponseMessage response = await client.PutAsJsonAsync("/api/products/1", changed);

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.AreEqual("Chai Latte", fake.Products.Single(p => p.ProductId == 1).ProductName);
    }

    [TestMethod]
    public async Task Update_Missing_Answers404()
    {
        HttpResponseMessage response = await client.PutAsJsonAsync("/api/products/999", NewProduct("Nothing"));

        AssertNotFoundFromTheAction(response);
    }

    [TestMethod]
    public async Task Delete_Existing_Answers204AndRemovesIt()
    {
        HttpResponseMessage response = await client.DeleteAsync("/api/products/2");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        Assert.IsFalse(fake.Products.Any(p => p.ProductId == 2));
    }

    [TestMethod]
    public async Task Delete_Missing_Answers404()
    {
        HttpResponseMessage response = await client.DeleteAsync("/api/products/999");

        AssertNotFoundFromTheAction(response);
    }

    [TestMethod]
    public async Task UnexpectedError_Answers500ProblemDetailsWithoutTheDetails()
    {
        HttpResponseMessage response = await client.GetAsync("/api/products/666");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("The fake database broke", body, "The client mustn't see the exception's details.");
    }

    [TestMethod]
    public void Program_RegistersTheRealProductDao()
    {
        using WebApplicationFactory<Program> realFactory = new WebApplicationFactory<Program>();
        using IServiceScope scope = realFactory.Services.CreateScope();

        IProductDao? dao = scope.ServiceProvider.GetService<IProductDao>();

        Assert.IsInstanceOfType<ProductDao>(dao, "Program.cs should register ProductDao as the IProductDao.");
    }

    [TestMethod]
    public void Program_RegistersTheDatabaseConflictHandler()
    {
        IEnumerable<IExceptionHandler> handlers = factory.Services.GetServices<IExceptionHandler>();

        Assert.IsTrue(handlers.Any(h => h is DatabaseConflictHandler), "Program.cs should register the DatabaseConflictHandler.");
    }
}
