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
        Task<List<eShopSelfVoucherListModel>> DisplayListOfActiveeVoucher();
    }
}
