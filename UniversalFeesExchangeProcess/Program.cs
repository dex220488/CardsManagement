using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using UniversalFeesExchangeProcess;

var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();
var sqlConnection = new SqliteConnection(app.GetConnectionString("CardsDB"));
Parallel.Invoke(() => CalculationMethods.UfeCalculation(sqlConnection));
Console.WriteLine("Process finished!");