using Cards.API.Entities.DTO;
using Cards.API.Interfaces;
using Cards.API.Services.Cards;
using Dapper;
using Microsoft.Data.Sqlite;
using ModelMinimalValidator;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(c => new SqliteConnection(builder.Configuration.GetConnectionString("CardsDB")));
builder.Services.AddScoped<IMinimalValidator, MinimalValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapSwagger();
app.UseSwaggerUI();

app.MapPost("api/cards", async (CardDTO card, SqliteConnection db, IMinimalValidator minimalValidator) =>
{
    var validationResult = minimalValidator.Validate(card);
    if (validationResult.IsValid)
    {
        ICardService cardService = new CardsService(db);
        var response = await cardService.Add(card);

        if (response.HasError)
        {
            return Results.Problem($"{response.ErrorMessage}", statusCode: 500);
        }

        return Results.Created($"/api/cards/", card.CardNumber);
    }

    return Results.ValidationProblem(validationResult.Errors);
});

app.MapPost("api/cards/pay", async (PurchaseTransactionDTO paymentInfo, SqliteConnection db) =>
{
    var sql = "INSERT INTO Cards(" +
                "CardNumber, CardholderName, ExpirationDate, CVV, Balance" +
              ") VALUES (" +
                "@CardNumber,@CardholderName,@ExpirationDate, @CVV, @Balance" +
               ");";

    await db.ExecuteAsync(sql, new
    {
        CardNumber = paymentInfo.CardNumber,
        CardholderName = "",
        ExpirationDate = paymentInfo.ExpirationDate,
        CVV = paymentInfo.CVV,
        Balance = 5
    });
});

app.MapGet("/api/cards", async (SqliteConnection db) =>
{
    ICardService cardService = new CardsService(db);
    return await cardService.GetAll();
});

app.MapGet("/api/cards/{cardNumber}", async (long cardNumber, SqliteConnection db) =>
{
    ICardService cardService = new CardsService(db);
    return await cardService.GetByCardNumber(cardNumber);
});

app.MapGet("/api/cards/{cardNumber}/balance", async (long cardNumber, SqliteConnection db) =>
{
    ICardService cardService = new CardsService(db);
    var data = await cardService.GetByCardNumber(cardNumber);

    if (data.Response != null)
    {
        return ((CardDTO)data.Response).Balance;
    }
    else
    {
        return 0;
    }
});

app.Run();