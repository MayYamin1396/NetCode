using eVoucher.DataInfrastructure;
using eVoucher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer.eStore
{
    public class eStoreBusinessLogic: IeStoreBusinessLogic
    {
        private eVoucherDbContext _dbContext;
        private IConfiguration configuration;
        public eStoreBusinessLogic(eVoucherDbContext context, IConfiguration config)
        {
            configuration = config;
            _dbContext = context;
        }
        public async Task<eShopSelfVoucherDetailModel> eVoucherDetail(int voucherID)
        {
            var getEVoucherbyID = await (
                from voucher in _dbContext.eVoucher
                .Where(vc => vc.ID == voucherID)
                orderby voucher.Title ascending
                from paymentMethod in _dbContext.paymentMethod 
                .Where(pm => pm.ID == voucher.PaymentMethodID )
                .DefaultIfEmpty()
                select new eShopSelfVoucherDetailModel
                {
                    ID = voucher.ID,
                    Title = voucher.Title,
                    Description = voucher.Description,
                    ExpireDate = voucher.ExpireDate,
                    ImageURL = voucher.ImageURL,
                    Amount = voucher.Amount,
                    PaymentMethod = paymentMethod.Method == null? "None": paymentMethod.Method,
                    Discount =
                    (
                        paymentMethod.DiscountType == "Fix" ? (paymentMethod.DiscountValue + " Ks"):
                        paymentMethod.DiscountType == "Pcent"? (paymentMethod.DiscountValue + "%") : "None"
                    ),
                    Stock = voucher.Quantity - (from orderCount in _dbContext.usersOrderedVouchers
                                                .Where(x => x.eVoucherID == voucher.ID) select orderCount)
                                                .Count(),
                    BuyType = voucher.BuyType,
                    RedeemPerUser = voucher.RedeemPerUser
                }).FirstOrDefaultAsync();

            return getEVoucherbyID;
        }
        public async Task<List<eShopSelfVoucherListModel>> DisplayListOfActiveeVoucher()
        {
            var getEVoucherbyID = await (
                from voucher in _dbContext.eVoucher
                .Where(vc => (vc.Quantity - (from orderCount in _dbContext.eVoucherQuantityControl
                                                .Where(x => x.VoucherID == vc.ID)
                                                  select orderCount.VoucherPurchasedQuantity).FirstOrDefault()) > 0)
                orderby voucher.Title ascending
                from paymentMethod in _dbContext.paymentMethod
                .Where(pm => pm.ID == voucher.PaymentMethodID)
                .DefaultIfEmpty()
                select new eShopSelfVoucherListModel
                {
                    ID = voucher.ID,
                    Title = voucher.Title,                 
                    ExpireDate = voucher.ExpireDate,
                    ImageURL = voucher.ImageURL,
                    Amount = voucher.Amount,
                    BuyType = voucher.BuyType
                }).ToListAsync();

            return getEVoucherbyID;
        }
        
    }
}
