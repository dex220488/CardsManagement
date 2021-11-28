using Microsoft.Data.Sqlite;

namespace UniversalFeesExchangeProcess
{
    internal class CalculationMethods
    {
        public static void UfeCalculation(SqliteConnection db)
        {
            UFESingleton ufe = UFESingleton.GetInstance;
            ufe.CalculationFee(db);
        }
    }
}