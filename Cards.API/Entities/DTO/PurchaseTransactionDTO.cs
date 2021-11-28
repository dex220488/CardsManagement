namespace Cards.API.Entities.DTO
{
    public class PurchaseTransactionDTO
    {
        public long CardNumber { get; set; }
        public int ExpirationDate { get; set; }
        public int CVV { get; set; }
        public string BusinessName { get; set; }
        public decimal Amount { get; set; }
    }
}