var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/books", () => "Hello World!");

app.Run();
