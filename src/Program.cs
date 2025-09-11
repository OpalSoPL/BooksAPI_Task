using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

AmazonDynamoDBConfig dbConfig = new AmazonDynamoDBConfig {ServiceURL = "http://localhost:8000"};
AmazonDynamoDBClient client = new AmazonDynamoDBClient(dbConfig);

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/books", async (BookData bookData) => postBooks(bookData));
app.MapGet("/books", async () => await getAllBooks());
app.MapGet("/books/{id}", async (int id) => await getBookByID(id));
app.MapDelete("/books/{id}", async (int id) => await deleteBook(id));

app.Run();

//api request handlers

//todo use DynamoDB

IResult postBooks(BookData data)
{
    return Results.Accepted();
}

async Task<IResult> getAllBooks()
{
    var request = new ScanRequest() { TableName = "Books" };

    var response = await client.ScanAsync(request);

    Dictionary<string, Dictionary<string, string>> data = new();

    foreach (var item in response.Items)
    {
        Dictionary<string, string> itemData = new();
        string id = item["Id"].S;

        foreach (var kvp in item)
        {
            if (kvp.Key.Equals("id")) continue;

            itemData.Add(kvp.Key, kvp.Value.S);
        }
        data.Add(id, itemData);
    }

    return Results.Json(data);
}

async Task<IResult> getBookByID(int id)
{
    var getRequest = new GetItemRequest
    {
        TableName = "Books",
        Key = new(){
            { "Id", new() { S = id.ToString() }}
        }
    };

    var getItemResponse = await client.GetItemAsync(getRequest);
    var item = getItemResponse.Item;

    if (item == null || item.Count == 0)
        return Results.NotFound();

    Dictionary<string, string> data = new();

    foreach (var kvp in item)
    {
        data.Add(kvp.Key, kvp.Value.S);
    }

    return Results.Json(data);
}

async Task<IResult> deleteBook(int id)
{

    return Results.Accepted($"/");
}

record BookData(string id, string author, string title);
