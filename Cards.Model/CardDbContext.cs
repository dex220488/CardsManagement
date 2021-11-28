using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Cards.Model
{
    public class CardDbContext : DbContext
    {
        public DbSet<Card> Cards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("FileName=CardDb.db", option =>
            {
                option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>().ToTable("Cards", "dbo");
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasKey(k => k.CardNumber);
            });
            modelBuilder.Entity<Card>().HasMany(f => f.PurchaseTransactions).WithOne();
            modelBuilder.Entity<Card>().HasMany(f => f.FeePayments).WithOne();

            modelBuilder.Entity<PurchaseTransaction>().ToTable("PurchaseTransactions", "dbo");
            modelBuilder.Entity<PurchaseTransaction>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<FeePayment>().ToTable("FeePayments", "dbo");
            modelBuilder.Entity<FeePayment>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<FeePayment>().HasMany(f => f.HistoricalFeePayments).WithOne();

            modelBuilder.Entity<HistoricalFeePayment>().ToTable("HistoricalFeePayments", "dbo");
            modelBuilder.Entity<HistoricalFeePayment>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class Card
    {
        public long CardNumber { get; set; }
        public string CardholderName { get; set; }
        public int ExpirationDate { get; set; }
        public int CVV { get; set; }
        public decimal Balance { get; set; }

        public List<PurchaseTransaction> PurchaseTransactions { get; set; }
        public List<FeePayment> FeePayments { get; set; }
    }

    public class PurchaseTransaction
    {
        public int Id { get; set; }
        public string BusinessName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class FeePayment
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public decimal CurrentFee { get; set; }
        public DateTime GeneratedFeeDateTime { get; set; }

        public List<HistoricalFeePayment> HistoricalFeePayments { get; set; }
    }

    public class HistoricalFeePayment
    {
        public long Id { get; set; }
        public decimal Fee { get; set; }
        public DateTime GeneratedDateTime { get; set; }
    }
}