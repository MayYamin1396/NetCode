using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public class EVoucherCMSModel
    {
    }
    public class eVoucherCreateRequestModel
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ExpireDate { get; set; }
        public string Base64Image { get; set; }
        public string Amount { get; set; }
        public string PaymentMethodID { get; set; }
        public string Quantity { get; set; }
        public string BuyType { get; set; }
        public string RedeemPerUser { get; set; }
        public string eStatus { get; set; }
    }
}
