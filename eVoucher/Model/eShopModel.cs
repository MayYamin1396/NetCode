using eVoucher.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public class eShopModel
    {
    }
    public class eShopSelfVoucherDetailModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ExpireDate { get; set; }
        public string ImageURL { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Discount { get; set; }
        public int Stock { get; set; }
        public string BuyType { get; set; }
        public int RedeemPerUser { get; set; }
    }
    public class eShopSelfVoucherListModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime ExpireDate { get; set; }
        public string ImageURL { get; set; }
        public decimal Amount { get; set; }
        public string BuyType { get; set; }
    }

    public class eShopDisplayActiveVoucherRequestModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [LengthValidation(10)]
        public string VoucherID { get; set; }
    }
    public class eShopDisplayActiveVoucherListByTypeRequestModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }

        [Required]
        [LengthValidation(20)]
        public string BuyType { get; set; }
    }
    public class eShopCheckOutRequestModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [LengthValidation(10)]
        public string VoucherID { get; set; }
        [Required]
        public string MobileNo { get; set; }

        [Required]
        [LengthValidation(4)]
        public string Quantity { get; set; }

        [Required]
        [LengthValidation(2)]
        public string PaymentMethodID { get; set; }
    }
    public class eShopTransactionRequestModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [Required]
        [LengthValidation(2)]
        public string PaymentMethodID { get; set; }
        public dynamic PaymentExtraData { get; set; }
        [Required]
        public eShopCheckOutAmountCalculationModel amountCalculationResult { get; set; }
    }
    public class eShopCheckOutAmountCalculationModel
    { 
        public string Amount { get; set; }
        public string Discount { get; set; }
        public string TotalAmount { get; set; }
        public string Quantity { get; set; }
        public string VoucherID { get; set; }
        public string ReceiverUserID { get; set; }
        public string TransactionID { get; set; }
    }
    public class eShopCheckOutResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public eShopCheckOutAmountCalculationModel data { get; set; }
    }
    public class eShopTransactionResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string TransactionStatus { get; set; } = "Pending";
    }
    public class eShopQRModel
    {
        public string PromoCode { get; set; }
        public string Amount { get; set; }
        public string Discount { get; set; }
        public string TotalAmount { get; set; }
        public string VoucherID { get; set; }
        public string OwnerID { get; set; }
        public string TransactionID { get; set; }
    }
    public class checkRemainerModel
    {
        public int remainer { get; set; }
    }
    public class checkQRModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        public string data { get; set; }
    }
    public class QRPromoCodeModel
    {
        public string PromoCode { get; set; }
        public string Amount { get; set; }
        public string Discount { get; set; }
        public string TotalAmount { get; set; }
        public string VoucherID { get; set; }
        public string OwnerID { get; set; }
        public string TransactionID { get; set; }
    }
    public class PromoCodeResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public dynamic data { get; set; }
    }
    public class PromoCodeStatusModel
    {
        public string PromoCode { get; set; }
        public string Amount { get; set; }
        public string TransactionID { get; set; }
        public string PromoCodeStatus { get; set; }
        public string Owner { get; set; }
    }
    public class ApplyPromoCodeModel
    {
        public string UserID { get; set; }
        public string ItemID { get; set; }
        public string PromoCode { get; set; }
    }
    public class TransactionHistoryModel
    {
        public string UserID { get; set; }
    }
    public class TransactionHistoryDetailModel
    {
        public string UserID { get; set; }
        public string TransactionID { get; set; }
    }
    public class ApplyPromoCodeResponseModel
    {
        public decimal Amount { get; set; }
        public decimal Charges { get; set; }
        public decimal PromoDiscount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class TransactionDetailResponseModel
    {
        public string QrCodeURL { get; set; }
        public string Promocode { get; set; }
        public string PromoStatus { get; set; }
        public string PromoAmount { get; set; }
    }
}
