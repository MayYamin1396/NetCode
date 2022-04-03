using eVoucher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer.eStore
{
    public interface IeStoreBusinessLogic
    {
        Task<eShopSelfVoucherDetailModel> eVoucherDetail(int voucherID);
        Task<List<eShopSelfVoucherListModel>> DisplayListOfActiveeVoucherByType(string BuyType);
        Task<eShopCheckOutResponseModel> eShopCheckOut(eShopCheckOutRequestModel requestModel);
        Task<eShopTransactionResponseModel> eShopTransaction(eShopTransactionRequestModel requestModel);
        Task<PromoCodeResponseModel> eShopValidatePromocode(checkQRModel requestModel);
        Task<PromoCodeResponseModel> eShopApplyPromocode(ApplyPromoCodeModel requestModel);
        Task<PromoCodeResponseModel> eShopPurchaseHistory(TransactionHistoryModel requestModel);
        Task<PromoCodeResponseModel> eShopPurchaseHistoryDetail(TransactionHistoryDetailModel requestModel);
    }
}
