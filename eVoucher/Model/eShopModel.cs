using System;
using System.Collections.Generic;
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

    public class eShopDisplayActiveVoucherRequestModel
    {
        public string UserID { get; set; }
        public string VoucherID { get; set; }
    }
}
