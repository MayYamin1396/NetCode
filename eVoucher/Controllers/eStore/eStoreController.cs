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
using Newtonsoft.Json;
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
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eVoucherDetail(int.Parse(requestModel.VoucherID),int.Parse(requestModel.UserID));
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);

            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("GetListOfVoucherByBuyType")]
        public async Task<IActionResult> GetListOfVoucherDetail([FromBody] eShopDisplayActiveVoucherListByTypeRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.DisplayListOfActiveeVoucherByType(requestModel.BuyType,int.Parse(requestModel.UserID));
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("CheckOutVoucher")]
        public async Task<IActionResult> CheckOutVoucher([FromBody] eShopCheckOutRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopCheckOut(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("MakePayment")]
        public async Task<IActionResult> MakePayment([FromBody] eShopTransactionRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopTransaction(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }


        [HttpPost]
        [Route("VerifyPromoCode")]
        public async Task<IActionResult> VerifyPromoCode([FromBody] checkQRModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopValidatePromocode(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = "0",
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }


        [HttpPost]
        [Route("ApplyPromoCode")]
        public async Task<IActionResult> ApplyPromoCode([FromBody] ApplyPromoCodeModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopApplyPromocode(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("PurchasedHistoryList")]
        public async Task<IActionResult> PurchasedHistoryList([FromBody] TransactionHistoryModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopPurchaseHistory(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }
        [HttpPost]
        [Route("PurchasedHistoryDetail")]
        public async Task<IActionResult> PurchasedHistoryDetail([FromBody] TransactionHistoryDetailModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopPurchaseHistoryDetail(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("GetPaymentMethodList")]
        public async Task<IActionResult> GetPaymentMethodList([FromBody] TransactionHistoryModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.eShopGetPaymentMethodList(requestModel);
                respData = JsonConvert.SerializeObject(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                await logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = JsonConvert.SerializeObject(requestModel),
                    ResponseData = respData,
                    CreatedDate = DateTime.Now,
                    Request_UserID = requestModel.UserID,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }
    }
}
