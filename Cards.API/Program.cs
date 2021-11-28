using Cards.Infrastructure.Entities.DTO;
using Cards.Infrastructure.Interfaces;
using Cards.Infrastructure.Services.Cards;
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

        if (cardService.ValidateExpirationDate(card.ExpirationDate))
        {
            var response = await cardService.Add(card);

            if (response.HasError)
            {
                return Results.Problem($"{response.ErrorMessage}", statusCode: 500);
            }

            return Results.Created($"/api/cards/", card.CardNumber);
        }
        else
        {
            return Results.Problem("Expiration Date is not valid");
        }
    }

    return Results.ValidationProblem(validationResult.Errors);
});

app.MapPost("api/cards/purchase", async (PurchaseTransactionDTO paymentInfo, SqliteConnection db, IMinimalValidator minimalValidator) =>
{
    var validationResult = minimalValidator.Validate(paymentInfo);
    if (validationResult.IsValid)
    {
        ICardService cardService = new CardsService(db);
        var response = await cardService.PurchaseTransaction(paymentInfo);

        if (response.HasError)
        {
            return Results.Problem($"{response.ErrorMessage}", statusCode: 500);
        }

        return Results.StatusCode(200);
    }

    return Results.ValidationProblem(validationResult.Errors);
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

    var response = new ResponseDTO();
    if (!data.HasError)
    {
        response.Response = ((CardDTO)data.Response).Balance;
    }
    else
    {
        response = data;
    }

    return response;
});

app.Run();