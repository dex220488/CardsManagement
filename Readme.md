# Card Management Services

Card managment services is a set of a demo solution with a management API module and a Universal Fees Exchange process.

## Prerequisites to Run 
- Visual Studio 2022
- .Net 6

## Installation 

- Rebuild the solution to restore all the nuget package dependencies
- Run the CreateSQLiteDatabase project to create the database with demo data and copy the connection string displayed in the console app
- Modify the connection string value located at the appsettings.json file in the projects Cards.API and UniversalFeesExchangeProcess

```

## Api Usage

- Run the project named GenerateNewBearerToken to generate a valid token. Copy the access token value.
- Run the project Cards.API
- Swagger information located at [API Documentation] (https://localhost:7224/Swagger/index.html)
- Configure Postman or your client sofwtare with the token generated by GenerateNewBearerToken as BearerToken
- Consume the Api Endpoint

```

## Universal Fees Exchange Usage

- Run the project named UniversalFeesExchangeProcess.
- Review the data using a sqlite tool.
- The console application can be published as an scheduled task and to be run every 1 hr.

## Sqlite queries to validate data

```bash
- SELECT * FROM Cards;
- SELECT * FROM PurchaseTransactions;
- SELECT * FROM FeePayments;
- SELECT * FROM HistoricalFeePayments;

```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.