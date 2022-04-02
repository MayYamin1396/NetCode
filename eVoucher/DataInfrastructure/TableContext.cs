using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.DataInfrastructure
{
    public class TableContext
    {
    }
    public class TransactionHistoryTableModel
    {
        [Key]
        public int ID { get; set; }
        public string TransactionID { get; set; }
        public int SenderUserID { get; set; }
        public int ReceiverUserID { get; set; }
        public string TransactionStatus { get; set; }
        public string ReversalTransactionID { get; set; }
        public string hasReversal { get; set; }
        public string OriginalAmount { get; set; }
        public string DiscountAmount { get; set; }
        public string TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class eVoucherTableModel
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ExpireDate { get; set; }
        public string ImageURL { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodID { get; set; }
        public int Quantity { get; set; }
        public string BuyType { get; set; }
        public int RedeemPerUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int eStatus { get; set; }
    }
    public class eVoucherLogTableModel
    {
        [Key]
        public int ID { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Request_UserID { get; set; }
        public string Message { get; set; }
    }
    public class UsersOrderedVouchersTableModel
    {
        [Key]
        public int ID { get; set; }
        public int eVoucherID { get; set; }
        public string QrCodeURL { get; set; }
        public string TransactionID { get; set; }      
        public string Promocode { get; set; }
    }
    public class PaymentMethodTableModel
    {
        [Key]
        public int ID { get; set; }
        public string Method { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MaxDiscount { get; set; }
        public decimal MinDiscount { get; set; }
    }
    public class UsersTableModel
    {
        [Key]
        public int ID { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
    }
}
