Dictionary<int, Dictionary<string, string>> tempDb = new ()
{
    {0, new Dictionary<string, string> {{"title", "losowy tytuł"}, {"author", "losowy autor"}}}
};

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/books", async () => postBooks());
app.MapGet("/books/{id}", async (int id) => getBookByID(id));
app.MapGet("/books", async () => getAllBooks());
app.MapDelete("/books/{id}", async (int id) => deleteBook(id));

app.Run();

//api request handlers

//todo use DynamoDB

IResult postBooks()
{
    return Results.Accepted();
}

IResult getAllBooks()
{
    return Results.Json(tempDb);
}

IResult getBookByID(int id)
{
    return Results.Json(tempDb[id]);
}

IResult deleteBook(int id)
{
    tempDb.Remove(id);
    return Results.Accepted();
}
