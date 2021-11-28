using Cards.Infrastructure.Entities.DTO;

namespace Cards.Infrastructure.Interfaces
{
    public interface ICardService
    {
        Task<ResponseDTO> Add(CardDTO data);

        Task<ResponseDTO> GetAll();

        Task<ResponseDTO> GetByCardNumber(long cardNumber);

        Task<ResponseDTO> PurchaseTransaction(PurchaseTransactionDTO data);

        bool ValidateExpirationDate(int expirationDate);
    }
}