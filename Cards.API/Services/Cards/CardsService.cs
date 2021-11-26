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
    }
}