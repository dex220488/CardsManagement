using Cards.API.Entities.DTO;

namespace Cards.API.Interfaces
{
    public interface ICardService
    {
        Task<ResponseDTO> Add(CardDTO data);
        Task<ResponseDTO> GetAll();
        Task<ResponseDTO> GetByCardNumber(long cardNumber);
    }
}