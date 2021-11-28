using Cards.Infrastructure.Entities.DTO;
using Cards.Infrastructure.Interfaces;
using Cards.Infrastructure.Services.Cards;
using Microsoft.Data.Sqlite;

namespace UniversalFeesExchangeProcess
{
    public sealed class UFESingleton
    {
        private UFESingleton(decimal random)
        {
            nextValue = random;
        }

        private static decimal nextValue;
        private static readonly Lazy<UFESingleton> Instancelock = new Lazy<UFESingleton>(() => new UFESingleton(new Random().Next(0, 20)));

        public static UFESingleton GetInstance
        {
            get
            {
                return Instancelock.Value;
            }
        }

        public async void CalculationFee(SqliteConnection db)
        {
            ICardService cardService = new CardsService(db);
            var feePayments = await cardService.GetAllFees();
            if (!feePayments.HasError)
            {
                List<FeePaymentDTO> allowedFeePayments = ((List<FeePaymentDTO>)feePayments.Response).Where(x => (DateTime.Now - x.GeneratedFeeDateTime).TotalHours >= 1).ToList();
                foreach (var item in allowedFeePayments)
                {
                    decimal nextDecimal = (nextValue / 100);
                    item.PreviousFee = item.CurrentFee;
                    item.CurrentFee = Decimal.Round(item.PreviousFee * nextDecimal, 2);
                    await cardService.UpdateFeePayment(item);
                }
            }
        }
    }
}