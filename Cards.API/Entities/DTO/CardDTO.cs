using System.ComponentModel.DataAnnotations;

namespace Cards.API.Entities.DTO
{
    public class CardDTO
    {
        [Required, Range(100000000000000, 999999999999999, ErrorMessage = "only 15 characters allowed")]
        public long CardNumber { get; set; }

        [Required]
        public string CardholderName { get; set; }

        [Required]
        public int ExpirationDate { get; set; }

        [Required]
        public int CVV { get; set; }

        public decimal Balance { get; set; }
    }
}