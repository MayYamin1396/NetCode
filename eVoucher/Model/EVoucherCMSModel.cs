using eVoucher.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public class EVoucherCMSModel
    {
    }
    public class eVoucherCreateRequestModel: ValidateThisModel
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [Required]
        [LengthValidation(100)]
        public string Password { get; set; }

        [Required]
        [LengthValidation(100)]
        public string Title { get; set; }
        [Required]
        [LengthValidation(1000)]
        public string Description { get; set; }
        [Required]
        public string ExpireDate { get; set; }
        [Required]
        public string Base64Image { get; set; }
        [Required]
        public string Amount { get; set; }
        public string PaymentMethodID { get; set; }
        [Required]
        public string Quantity { get; set; }
        [Required]
        public string BuyType { get; set; }
        [Required]
        public string RedeemPerUser { get; set; }
    }
}
