using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

// The products API.
//
// Requirements:
// - GET /api/products answers 200 with every product. GET /api/products?categoryId=1 answers with only
//   that category's products.
// - GET /api/products/{id} answers 200 with the product, or 404 Not Found if there's no such product.
// - POST /api/products adds the product in the body and answers 201 Created, with the new product
//   (and its new id) in the body and its address in the Location header. A product that breaks the
//   Product class's validation rules answers 400 Bad Request without being saved.
// - PUT /api/products/{id} replaces the product's values with the body's, and answers 204 No Content,
//   or 404 Not Found if there's no such product.
// - DELETE /api/products/{id} deletes the product and answers 204 No Content, or 404 Not Found if
//   there's no such product.
// - Every action is async, and uses the IProductDao it's given.
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductDao productDao;

    public ProductsController(IProductDao productDao)
    {
        this.productDao = productDao;
    }
}
