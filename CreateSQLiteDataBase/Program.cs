// See https://aka.ms/new-console-template for more information
using Cards.Model;

Console.WriteLine("Starting DataBase Creation!");
var dbName = "CardDb.db";

if (File.Exists(dbName))
    File.Delete(dbName);

await using var dbContext = new CardDbContext();
await dbContext.Database.EnsureCreatedAsync();

Console.WriteLine("Database created");

var feePayments = new List<Cards.Model.FeePayment>();
feePayments.Add(new Cards.Model.FeePayment()
{
    Description = "Fee Payment #1",
    CurrentFee = 250.75M,
    GeneratedFeeDateTime = DateTime.Now
});

feePayments.Add(new Cards.Model.FeePayment()
{
    Description = "Fee Payment #2",
    CurrentFee = 50.45M,
    GeneratedFeeDateTime = DateTime.Now
});

var purchaseTransactions = new List<Cards.Model.PurchaseTransaction>();
purchaseTransactions.Add(new Cards.Model.PurchaseTransaction()
{
    BusinessName = "Ecommerce #1",
    Amount = 1500.55M,
    TransactionDate = DateTime.Now
});

purchaseTransactions.Add(new Cards.Model.PurchaseTransaction()
{
    BusinessName = "Ecommerce #2",
    Amount = 2500.45M,
    TransactionDate = DateTime.Now
});

dbContext.Cards.Add(new Card()
{
    CardNumber = 123456789012345,
    CardholderName = "Customer Name 1",
    ExpirationDate = 202111,
    CVV = 123,
    Balance = 5000,
    FeePayments = feePayments,
    PurchaseTransactions = purchaseTransactions
});

dbContext.SaveChanges();

Console.WriteLine("Demo data inserted");