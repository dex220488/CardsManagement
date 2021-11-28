using Cards.Infrastructure.Entities.DTO;

namespace Cards.Infrastructure.Interfaces
{
    public interface ICardService
    {
        Task<ResponseDTO> Add(CardDTO data);

        Task<ResponseDTO> GetAllCards();

        Task<ResponseDTO> GetByCardNumber(long cardNumber);

        Task<ResponseDTO> PurchaseTransaction(PurchaseTransactionDTO data);

        Task<ResponseDTO> GetAllFees();

        Task<ResponseDTO> UpdateFeePayment(FeePaymentDTO data);

        bool ValidateExpirationDate(int expirationDate);
    }
}