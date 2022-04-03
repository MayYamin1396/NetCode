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
        Task<APIResponseWithDataModel> GetListOfEVoucher(eVoucherDisplayByIDRequestModel requestModel);
        Task<APIResponseWithDataModel> GetEVoucherByID(eVoucherDisplayByIDRequestModel requestModel);
        Task<APIResponseModel> DeactivateEVoucher(eVoucherDeactivateRequestModel requestModel);
    }
}
