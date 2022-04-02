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
                                                  select orderCount.VoucherPurchasedQuantity).FirstOrDefault()) > 0 && vc.eStatus == 1)
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
        public async Task<eShopCheckOutResponseModel> eShopCheckOut(eShopCheckOutRequestModel requestModel)
        {
            var calcualtionModel = new eShopCheckOutAmountCalculationModel();

            #region Validations
            var isDiscountAppealable = await (
                from Pmethod in _dbContext.eVoucher
                .Where(vc => vc.ID == int.Parse(requestModel.VoucherID) && vc.eStatus == 1)
                select Pmethod).FirstOrDefaultAsync();
            if(isDiscountAppealable == null)
            {
                return new eShopCheckOutResponseModel { ResponseCode = "014", ResponseDescription = "This eVoucher is deactivated." };
            }
            if(isDiscountAppealable.Quantity < int.Parse(requestModel.Quantity))
            {
                return new eShopCheckOutResponseModel { ResponseCode = "015", ResponseDescription = "You have entered more than available quantity." };
            }
            #endregion

            var PreTotalAmount = int.Parse(requestModel.Quantity) * isDiscountAppealable.Amount;
            decimal DiscountValue = 0;          
            if (isDiscountAppealable.PaymentMethodID.ToString() == requestModel.PaymentMethodID)
            {
                DiscountValue = await DiscountAmountCalculation(requestModel.PaymentMethodID, PreTotalAmount);                
            }
            calcualtionModel.Amount = isDiscountAppealable.Amount.ToString();
            calcualtionModel.Discount = DiscountValue.ToString();
            calcualtionModel.TotalAmount = (PreTotalAmount - DiscountValue).ToString();
            calcualtionModel.Quantity = requestModel.Quantity;

            return new eShopCheckOutResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = calcualtionModel }  ;
        }
        public async Task<decimal> DiscountAmountCalculation(string PaymentMethodID, decimal TotalAmount)
        {
            var getDiscount = await(
                from Pmethod in _dbContext.paymentMethod
                .Where(vc => vc.ID == int.Parse(PaymentMethodID) && vc.MethodStatus == 1)
                select Pmethod).FirstOrDefaultAsync();

            decimal discountValue = 0;
            string DiscountType = null;
            if (getDiscount != null)
            {
                DiscountType = getDiscount.DiscountType;
                discountValue = getDiscount.DiscountValue;

            }
            
            return GetDiscountAmount(DiscountType, TotalAmount, discountValue);

        }
        public static decimal GetDiscountAmount(string discountType, decimal amount, decimal discountvalue)
        {          
            decimal returnAmount = 0;
            if (discountType == "Pcent")
            {
                returnAmount = amount * (discountvalue / 100);
            }
            else if (discountType == "2")
            {
                returnAmount = discountvalue;
            }
            return returnAmount;
        }
    }
}
