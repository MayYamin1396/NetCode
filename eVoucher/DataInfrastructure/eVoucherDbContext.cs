using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.DataInfrastructure
{
    public class eVoucherDbContext: DbContext
    {
        public eVoucherDbContext(DbContextOptions<eVoucherDbContext> options) : base(options) { }
        public DbSet<TransactionHistoryTableModel> transactionHistory { get; set; }
        public DbSet<eVoucherTableModel> eVoucher { get; set; }
        public DbSet<eVoucherLogTableModel> eVoucherLog { get; set; }
        public DbSet<UsersOrderedVouchersTableModel> usersOrderedVouchers { get; set; }
        public DbSet<PaymentMethodTableModel> paymentMethod { get; set; }
        public DbSet<UsersTableModel> usersTableModel { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TransactionHistoryTableModel>().ToTable("transactionHistory");
            modelBuilder.Entity<eVoucherTableModel>().ToTable("eVoucher");
            modelBuilder.Entity<eVoucherLogTableModel>().ToTable("eVoucherLog");
            modelBuilder.Entity<UsersOrderedVouchersTableModel>().ToTable("usersOrderedVouchers");
            modelBuilder.Entity<PaymentMethodTableModel>().ToTable("paymentMethod");
            modelBuilder.Entity<UsersTableModel>().ToTable("usersTableModel");

        }
    }
}
