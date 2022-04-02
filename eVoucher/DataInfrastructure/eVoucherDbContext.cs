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
        public DbSet<eVoucherQuantityControl> eVoucherQuantityControl { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TransactionHistoryTableModel>().ToTable("e_voucher_transactionhistory");
            modelBuilder.Entity<eVoucherTableModel>().ToTable("evoucher");
            modelBuilder.Entity<eVoucherLogTableModel>().ToTable("evoucherlog");
            modelBuilder.Entity<UsersOrderedVouchersTableModel>().ToTable("orderedevoucher");
            modelBuilder.Entity<PaymentMethodTableModel>().ToTable("paymentmethod");
            modelBuilder.Entity<UsersTableModel>().ToTable("users");
            modelBuilder.Entity<eVoucherQuantityControl>().ToTable("evoucher_control");
        }
    }
}
