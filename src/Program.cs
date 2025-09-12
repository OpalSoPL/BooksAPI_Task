using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

AmazonDynamoDBClient client = new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig {ServiceURL = "http://localhost:8000"}
    );

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/books", async (BookData bookData) => await postBooks(bookData));
app.MapGet("/books", async () => await getAllBooks());
app.MapGet("/books/{id}", async (string id) => await getBookByID(id));
app.MapDelete("/books/{id}", async (string id) => await deleteBook(id));

app.Run();

//api request handlers

async Task<IResult> postBooks(BookData data)
{
    var request = new PutItemRequest
    {
        TableName = "Books",
        Item = new()
        {
            {"Id", new() { S = data.Id }},
            {"Author", new() { S = data.Author }},
            {"Title", new () { S = data.Title }}
        }
    };

    try
    {
        await client.PutItemAsync(request);
        return Results.Created();
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

async Task<IResult> getAllBooks()
{
    var request = new ScanRequest() { TableName = "Books" };

    try
    {
        var response = await client.ScanAsync(request);

        Dictionary<string, Dictionary<string, string>> parsedResponse = new();

        //parse data from AWS response
        foreach (var item in response.Items)
        {
            Dictionary<string, string> parsedItem = new();

            foreach (var kvp in item)
            {
                if (kvp.Key.Equals("Id")) continue;

                parsedItem.Add(kvp.Key, kvp.Value.S);
            }

            string id = item["Id"].S;
            parsedResponse.Add(id, parsedItem);
        }

        return Results.Json(parsedResponse);
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }

}

async Task<IResult> getBookByID(string id)
{
    if (!int.TryParse(id, out _))
        return Results.BadRequest();

    var getRequest = new GetItemRequest
    {
        TableName = "Books",
        Key = new() {{ "Id", new() { S = id }}}
    };

    try
    {
        var response = await client.GetItemAsync(getRequest);
        var item = response.Item;

        if (item == null || item.Count == 0)
            return Results.NotFound();

        Dictionary<string, string> data = new();

        foreach (var kvp in item)
            data.Add(kvp.Key, kvp.Value.S);

        return Results.Json(data);
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

async Task<IResult> deleteBook(string id)
{
    if (!int.TryParse(id, out _))
        return Results.BadRequest();

    var request = new DeleteItemRequest
    {
        TableName = "Books",
        Key = new() {{ "Id", new () { S = id }}}
    };

    try
    {
        await client.DeleteItemAsync(request);
        return Results.NoContent();
    }
    catch (AmazonDynamoDBException e)
    {
        return Results.Problem(e.Message, statusCode: (int)e.StatusCode);
    }
}

record BookData(string Id, string Author, string Title);
