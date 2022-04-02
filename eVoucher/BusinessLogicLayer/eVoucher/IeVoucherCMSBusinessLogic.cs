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
        Task<APIResponseModel> CreateEVoucher(eVoucherCreateAndUpdateRequestModel requestModel);
        Task<APIResponseModel> UpdateEVoucher(eVoucherCreateAndUpdateRequestModel requestModel);
        Task<List<eVoucherTableModel>> GetListOfEVoucher();
        Task<eVoucherTableModel> GetEVoucherByID(eVoucherDisplayByIDRequestModel requestModel);
        Task<APIResponseModel> DeactivateEVoucher(eVoucherDeactivateRequestModel requestModel);
    }
}
