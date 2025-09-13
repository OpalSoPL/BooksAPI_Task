using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

//DynamoDB setup
AmazonDynamoDBClient client = new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig {ServiceURL = "http://localhost:8000"}
    );

TableBuilder BookTableBuilder = new(client, "Books");
    BookTableBuilder.AddHashKey("Id", DynamoDBEntryType.String);

//ASP.NET setup

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/books", async (BookData bookData) => await postBooks(bookData.ToDocument()));
app.MapGet("/books", async () => await getAllBooks());
app.MapGet("/books/{id}", async (string id) => await getBookByID(id));
app.MapDelete("/books/{id}", async (string id) => await deleteBook(id));

app.Run();

//api request handlers

async Task<IResult> postBooks(Document data)
{
    Table table = BookTableBuilder.Build();

    try
    {
        await table.PutItemAsync(data);
        return Results.Created();
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

async Task<IResult> getAllBooks()
{
    Table table = BookTableBuilder.Build();

    try
    {
        var search = table.Scan(new ScanFilter());
        List<BookData> list = [];

        //parse data from AWS response
        while (!search.IsDone)
        {
            var set = await search.GetNextSetAsync();

            list.AddRange(set.Select(data => BookData.Convert(data)));
        }
        return Results.Json(list);
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }

}

async Task<IResult> getBookByID(string id)
{
    Table table = BookTableBuilder.Build();

    try
    {
        Document res = await table.GetItemAsync(id);

        if (res == null)
            return Results.NotFound();

        return Results.Json(BookData.Convert(res));
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

async Task<IResult> deleteBook(string id)
{
    Table table = BookTableBuilder.Build();
    try
    {
        await table.DeleteItemAsync(id);
        return Results.NoContent();
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

record BookData(string Id, string Author, string Title)
{
    public static BookData Convert(Document doc)
    {
        return new BookData(doc["Id"], doc["Author"], doc["Title"]);
    }

    public Document ToDocument()
    {
        return new()
        {
            ["Id"] = Id,
            ["Author"] = Author,
            ["Title"] = Title
        };
    }
}
