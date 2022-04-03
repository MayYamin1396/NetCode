using eVoucher.Authentication;
using eVoucher.BusinessLogicLayer;
using eVoucher.BusinessLogicLayer.eStore;
using eVoucher.DataInfrastructure;
using eVoucher.Helpers.Logging;
using eVoucher.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Controllers.eStore
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/eStore")]
    public class eStoreController : BaseAPIController
    {
        private readonly iJWTAuthentication authentication;
        private readonly IConfiguration config;
        private readonly eVoucherDbContext _Dbcontext;
        private readonly IeStoreBusinessLogic businessLayer;
        private readonly ILogServices logging;
        public eStoreController(ILogServices log, IeStoreBusinessLogic business, iJWTAuthentication auth, IConfiguration configuration, eVoucherDbContext Dbcontext)
        {
            logging = log;
            businessLayer = business;
            authentication = auth;
            config = configuration;
            _Dbcontext = Dbcontext;
        }
        [HttpPost]
        [Route("GetVoucherDetail")]
        public async Task<IActionResult> GetListOfActiveVoucher([FromBody] eShopDisplayActiveVoucherRequestModel requestModel)
        {
            string DecryptRequest = null;
            string getToken = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var CreateVoucher = await businessLayer.eVoucherDetail(int.Parse(requestModel.VoucherID));
                return Ok(CreateVoucher);

            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                //new Thread(() => logging.PerformLoggingAsync(new eVoucherLogTableModel
                //{
                //    Method = Request.Method,
                //    Route = Request.Path,
                //    RequestData = DecryptRequest,
                //    ResponseData = JsonConvert.SerializeObject(getToken),
                //    CreatedDate = DateTime.Now,
                //    Request_UserID = 0,
                //    Message = errMessage == null ? "Success" : errMessage

                //})).Start();
            }
        }

        [HttpPost]
        [Route("GetListOfVoucherByBuyType")]
        public async Task<IActionResult> GetListOfVoucherDetail([FromBody] eShopDisplayActiveVoucherListByTypeRequestModel requestModel)
        {
            string DecryptRequest = null;
            string getToken = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var CreateVoucher = await businessLayer.DisplayListOfActiveeVoucherByType(requestModel.BuyType);
                return Ok(CreateVoucher);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                //new Thread(() => logging.PerformLoggingAsync(new eVoucherLogTableModel
                //{
                //    Method = Request.Method,
                //    Route = Request.Path,
                //    RequestData = DecryptRequest,
                //    ResponseData = JsonConvert.SerializeObject(getToken),
                //    CreatedDate = DateTime.Now,
                //    Request_UserID = 0,
                //    Message = errMessage == null ? "Success" : errMessage

                //})).Start();
            }
        }

        [HttpPost]
        [Route("CheckOutVoucher")]
        public async Task<IActionResult> CheckOutVoucher([FromBody] eShopCheckOutRequestModel requestModel)
        {
            string DecryptRequest = null;
            string getToken = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var CreateVoucher = await businessLayer.eShopCheckOut(requestModel);
                return Ok(CreateVoucher);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                //new Thread(() => logging.PerformLoggingAsync(new eVoucherLogTableModel
                //{
                //    Method = Request.Method,
                //    Route = Request.Path,
                //    RequestData = DecryptRequest,
                //    ResponseData = JsonConvert.SerializeObject(getToken),
                //    CreatedDate = DateTime.Now,
                //    Request_UserID = 0,
                //    Message = errMessage == null ? "Success" : errMessage

                //})).Start();
            }
        }

        [HttpPost]
        [Route("MakePayment")]
        public async Task<IActionResult> MakePayment([FromBody] eShopTransactionRequestModel requestModel)
        {
            string DecryptRequest = null;
            string getToken = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var CreateVoucher = await businessLayer.eShopTransaction(requestModel);
                return Ok(CreateVoucher);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                //new Thread(() => logging.PerformLoggingAsync(new eVoucherLogTableModel
                //{
                //    Method = Request.Method,
                //    Route = Request.Path,
                //    RequestData = DecryptRequest,
                //    ResponseData = JsonConvert.SerializeObject(getToken),
                //    CreatedDate = DateTime.Now,
                //    Request_UserID = 0,
                //    Message = errMessage == null ? "Success" : errMessage

                //})).Start();
            }
        }

    }
}
