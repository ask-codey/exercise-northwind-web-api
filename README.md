# Practice: Northwind Web API

Practice code for the lesson **Practice: Northwind Web API** in the C# Full-Stack Bootcamp on [ask-codey.com](https://www.ask-codey.com).

Northwind's products need an API, so other programs can read and change them. The DAO is written; you'll write the controller, and set up `Program.cs` so the API registers its DAO and handles errors in one place.

## What's here

- `NorthwindApi/Models/Product.cs`: a product, with its validation rules. Ready to use.
- `NorthwindApi/Data/IProductDao.cs`, `ProductDao.cs`: the products table, with Dapper and async methods. Ready to use.
- `NorthwindApi/Errors/DatabaseConflictHandler.cs`: answers SQL Server's constraint errors with 409 Conflict. Ready to use: register it.
- `NorthwindApi/Controllers/ProductsController.cs`: for you to finish. The requirements are in the comment at the top.
- `NorthwindApi/Program.cs`: for you to finish, where the `TODO` comments are.
- `NorthwindApi/NorthwindApi.http`: ten numbered requests to try your API by hand, each with the answer to expect.
- `NorthwindApi.Tests`: tests that start the API in memory with a fake DAO, so they need no database. You don't need to change these.

## Before you start

The API uses your own `Northwind` database. If yours has another name, change the connection string in `appsettings.json`.

## What to do

1. **`Program.cs`:** register `ProductDao` as the `IProductDao`, add problem details and the `DatabaseConflictHandler`, and handle exceptions for the whole API.
2. **`ProductsController`:** `GetAll` (with an optional `categoryId`), `GetById`, `Add`, `Update` and `Delete`, all async.

Run the tests with **Test > Run All Tests** (or `dotnet test`). Then run the API with **Ctrl+F5**, open `NorthwindApi.http`, and send the requests in order.

## Tips

- An optional query string value is a nullable parameter: `int? categoryId`.
- `{id:int}` in a route template only matches whole numbers.
- `CreatedAtAction(nameof(GetById), new { id = added.ProductId }, added)` answers 201 with the Location header.
- `[ApiController]` answers 400 for invalid products by itself; you don't need to check `ModelState`.
- `builder.Services.AddProblemDetails()`, `builder.Services.AddExceptionHandler<DatabaseConflictHandler>()`, and `app.UseExceptionHandler()` right after `builder.Build()`.

## Hand it in

When it works, push your solution to a public repo of your own on GitHub, then paste that repo's address into **Submit your exercise** on the lesson page at [ask-codey.com](https://www.ask-codey.com). That's what completes the lesson. The lesson page shows the Git commands, step by step.
