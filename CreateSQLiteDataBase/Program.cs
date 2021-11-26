// See https://aka.ms/new-console-template for more information
using Cards.Model;

Console.WriteLine("Starting DataBase Creation!");
var dbName = "CardDb.db";

if (File.Exists(dbName))
    File.Delete(dbName);

await using var dbContext = new CardDbContext();
await dbContext.Database.EnsureCreatedAsync();

Console.WriteLine("Database created");