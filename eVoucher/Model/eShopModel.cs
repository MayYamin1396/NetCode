﻿using eVoucher.Validators;
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
    public class eShopCheckOutRequestModel : ValidateThisModel /// Flag to check the Attributes 
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [LengthValidation(10)]
        public string VoucherID { get; set; }

        [Required]
        [LengthValidation(4)]
        public string Quantity { get; set; }

        [Required]
        [LengthValidation(2)]
        public string PaymentMethodID { get; set; }
    }
    public class eShopCheckOutAmountCalculationModel
    { 
        public string Amount { get; set; }
        public string Discount { get; set; }
        public string TotalAmount { get; set; }
        public string Quantity { get; set; }
    }
    public class eShopCheckOutResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public eShopCheckOutAmountCalculationModel data { get; set; }
    }
}
