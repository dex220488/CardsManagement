using Cards.API.Entities.DTO;
using Cards.API.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cards.API.Services.Cards
{
    public class CardsService : ICardService
    {
        private SqliteConnection _db;

        public CardsService(SqliteConnection db)
        {
            _db = db;
        }

        public async Task<ResponseDTO> Add(CardDTO data)
        {
            var response = new ResponseDTO();
            try
            {
                var sql = "INSERT INTO Cards(" +
                            "CardNumber, CardholderName, ExpirationDate, CVV, Balance" +
                          ") VALUES (" +
                            "@CardNumber,@CardholderName,@ExpirationDate, @CVV, @Balance" +
                          ");";

                await _db.ExecuteAsync(sql, new
                {
                    CardNumber = data.CardNumber,
                    CardholderName = data.CardholderName,
                    ExpirationDate = data.ExpirationDate,
                    CVV = data.CVV,
                    Balance = data.Balance
                });
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ErrorMessage = $"ErrorMessage => {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.ErrorMessage = $"{response.ErrorMessage} :: InnerException => {ex.InnerException.Message}";
                }

                response.ErrorMessage = $"{response.ErrorMessage} :: StackTrace => {ex.StackTrace}";
            }

            return response;
        }

        public async Task<ResponseDTO> GetAll()
        {
            var response = new ResponseDTO();
            try
            {
                var sql = "SELECT * FROM Cards;";
                var data = await _db.QueryAsync<CardDTO>(sql);
                response.Response = data;
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ErrorMessage = $"ErrorMessage => {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.ErrorMessage = $"{response.ErrorMessage} :: InnerException => {ex.InnerException.Message}";
                }

                response.ErrorMessage = $"{response.ErrorMessage} :: StackTrace => {ex.StackTrace}";
            }

            return response;
        }

        public async Task<ResponseDTO> GetByCardNumber(long cardNumber)
        {
            var response = new ResponseDTO();
            try
            {
                var sql = "SELECT * FROM Cards WHERE CardNumber = @cardNumber";
                var data = await _db.QuerySingleOrDefaultAsync<CardDTO>(sql, new { cardNumber });

                if (data != null)
                    response.Response = data;
                else
                {
                    response.HasError = true;
                    response.ErrorMessage = $"No information is available for the indicated card number {cardNumber}";
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ErrorMessage = $"ErrorMessage => {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.ErrorMessage = $"{response.ErrorMessage} :: InnerException => {ex.InnerException.Message}";
                }

                response.ErrorMessage = $"{response.ErrorMessage} :: StackTrace => {ex.StackTrace}";
            }

            return response;
        }

        public async Task<ResponseDTO> PurchaseTransaction(PurchaseTransactionDTO data)
        {
            var response = new ResponseDTO();
            try
            {
                if (await ValidateExistingCard(data))
                {
                    if (!await ValidateCreditIsOverdrawn(data.CardNumber, data.Amount))
                    {
                        await _db.OpenAsync().ConfigureAwait(false);
                        using (var transaction = await _db.BeginTransactionAsync())
                        {
                            var sql = "INSERT INTO PurchaseTransactions(" +
                                    "CardNumber, BusinessName, TransactionDate, Amount" +
                                  ") VALUES (" +
                                    "@CardNumber,@BusinessName,@TransactionDate, @Amount" +
                                  ");";

                            await _db.ExecuteAsync(sql, new
                            {
                                CardNumber = data.CardNumber,
                                BusinessName = data.BusinessName,
                                TransactionDate = DateTime.Now,
                                Amount = data.Amount
                            });

                            sql = "UPDATE Cards " +
                                   "SET Balance = Balance - @Amount " +
                                 " WHERE " +
                                   "CardNumber = @CardNumber AND ExpirationDate = @ExpirationDate AND CVV = @CVV;";

                            await _db.ExecuteAsync(sql, new
                            {
                                CardNumber = data.CardNumber,
                                ExpirationDate = data.ExpirationDate,
                                CVV = data.CVV,
                                Amount = data.Amount
                            });

                            transaction.Commit();
                        }
                    }
                    else
                    {
                        response.HasError = true;
                        response.ErrorMessage = "The card has exceeded its credit limit";
                    }
                }
                else
                {
                    response.HasError = true;
                    response.ErrorMessage = "Card Information not valid or non-existent";
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ErrorMessage = $"ErrorMessage => {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.ErrorMessage = $"{response.ErrorMessage} :: InnerException => {ex.InnerException.Message}";
                }

                response.ErrorMessage = $"{response.ErrorMessage} :: StackTrace => {ex.StackTrace}";
            }

            return response;
        }

        private async Task<bool> ValidateExistingCard(PurchaseTransactionDTO cardInfo)
        {
            var sql = "SELECT * FROM Cards WHERE CardNumber = @CardNumber AND ExpirationDate = @ExpirationDate AND CVV = @CVV ";
            var data = await _db.QuerySingleOrDefaultAsync<CardDTO>(sql, new
            {
                CardNumber = cardInfo.CardNumber,
                ExpirationDate = cardInfo.ExpirationDate,
                CVV = cardInfo.CVV
            });

            if (data == null)
                return false;
            else
                return true;
        }

        private async Task<bool> ValidateCreditIsOverdrawn(long cardNumber, decimal transactionAmount)
        {
            bool isOverdrawn = false;
            var cardInfo = await GetByCardNumber(cardNumber);

            if (cardInfo.Response != null)
            {
                var cardDto = (CardDTO)cardInfo.Response;
                if (cardDto.Balance - transactionAmount <= 0)
                    isOverdrawn = true;
            }

            return isOverdrawn;
        }

        public bool ValidateExpirationDate(int expirationDate)
        {
            int.TryParse(expirationDate.ToString()[..4], out int year);
            int.TryParse(expirationDate.ToString()[4..], out int month);

            bool isValid = false;

            // If month from expiration date is in a valid range number
            if (month > 1 && month < 13)
                isValid = true;

            if (isValid)
            {
                // if year is lesser to current year
                if (year < DateTime.Now.Year)
                    isValid = false;
                // If current year and month is not expired
                if (year == DateTime.Now.Year && month > DateTime.Now.Month)
                    isValid = true;
                else if (year == DateTime.Now.Year && month <= DateTime.Now.Month)
                    isValid = false;
                // If expiration year is greater than current year
                else if (year > DateTime.Now.Year)
                    isValid = true;
            }

            return isValid;
        }
    }
}