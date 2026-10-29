var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// TODO: register IProductDao, problem details and the DatabaseConflictHandler.

var app = builder.Build();

// TODO: handle exceptions for the whole API.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the tests start the app.
public partial class Program;
