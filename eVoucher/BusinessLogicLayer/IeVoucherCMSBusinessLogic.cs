using eVoucher.DataInfrastructure;
using eVoucher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer
{
    public interface IeVoucherCMSBusinessLogic
    {
        Task<APIResponseModel> CreateEVoucher(eVoucherCreateRequestModel requestModel);
        Task<List<eVoucherTableModel>> getListOfToken();
    }
}
