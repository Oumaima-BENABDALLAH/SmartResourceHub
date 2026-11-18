using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payment.Application.Persistence
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<ProcessedTransactionEntity> ProcessedTransactions => Set<ProcessedTransactionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedTransactionEntity>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
        }
    }
}
