namespace Cards.Infrastructure.Entities.DTO
{
    public class FeePaymentDTO
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public decimal CurrentFee { get; set; }
        public decimal PreviousFee { get; set; }
        public DateTime GeneratedFeeDateTime { get; set; }
    }
}