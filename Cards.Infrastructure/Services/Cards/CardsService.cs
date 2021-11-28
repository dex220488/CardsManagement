using Cards.Infrastructure.Entities.DTO;
using Cards.Infrastructure.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cards.Infrastructure.Services.Cards
{
    public class CardsService : ICardService
    {
        private SqliteConnection _db;

        public CardsService(SqliteConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new card in the module
        /// </summary>
        /// <param name="data">Card Information</param>
        /// <returns>Generic ResponseDTO with errors if any</returns>
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

        /// <summary>
        /// Get the complete list of cards
        /// </summary>
        /// <returns>List of CardDTO inside Response Property. Generic ResponseDTO with errors if any</returns>
        public async Task<ResponseDTO> GetAllCards()
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

        /// <summary>
        /// Get information from a specific card
        /// </summary>
        /// <param name="cardNumber">Card number</param>
        /// <returns>CardDTO inside Response Property. Generic ResponseDTO with errors if any</returns>
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

        /// <summary>
        /// Makes a purchase and updates card balance
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Generic ResponseDTO with errors if any</returns>
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

        /// <summary>
        /// Validates if a card exists
        /// </summary>
        /// <param name="cardInfo">Card information</param>
        /// <returns>true if exists or false if it doesn't exist</returns>
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

        /// <summary>
        /// Validates if a card will be overdrawn for a new transaction
        /// </summary>
        /// <param name="cardNumber">Card number</param>
        /// <param name="transactionAmount">Amount of new transaction</param>
        /// <returns>true if it is overdrawn</returns>
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

        /// <summary>
        /// Validates an expiration date
        /// </summary>
        /// <param name="expirationDate">Expiration date</param>
        /// <returns>true if it is a valid value</returns>
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

        /// <summary>
        /// Get all fee payments
        /// </summary>
        /// <returns>List of FeePaymentDTO inside Response Property. Generic ResponseDTO with errors if any</returns>
        public async Task<ResponseDTO> GetAllFees()
        {
            var response = new ResponseDTO();
            try
            {
                var sql = "SELECT * from FeePayments";
                var data = await _db.QueryAsync<FeePaymentDTO>(sql);
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

        /// <summary>
        /// Update a fee payment
        /// </summary>
        /// <param name="data">Data to be updated</param>
        /// <returns>Generic ResponseDTO with errors if any</returns>
        public async Task<ResponseDTO> UpdateFeePayment(FeePaymentDTO data)
        {
            var response = new ResponseDTO();
            try
            {
                await _db.OpenAsync().ConfigureAwait(false);
                using (var transaction = await _db.BeginTransactionAsync())
                {
                    var sql = "INSERT INTO HistoricalFeePayments(" +
                            "FeePaymentId, Fee, GeneratedDateTime" +
                          ") VALUES (" +
                            "@FeeePaymentId,@Fee,@GeneratedDateTime" +
                          ");";

                    await _db.ExecuteAsync(sql, new
                    {
                        FeeePaymentId = data.Id,
                        Fee = data.PreviousFee,
                        GeneratedDateTime = data.GeneratedFeeDateTime
                    });

                    sql = "UPDATE FeePayments " +
                           "SET CurrentFee = @CurrentFee, GeneratedFeeDateTime =  @GeneratedFeeDateTime" +
                         " WHERE " +
                           "Id = @Id;";

                    await _db.ExecuteAsync(sql, new
                    {
                        CurrentFee = data.CurrentFee,
                        GeneratedFeeDateTime = DateTime.Now,
                        Id = data.Id
                    });

                    transaction.Commit();
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
    }
}