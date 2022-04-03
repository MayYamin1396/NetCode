using eVoucher.Authentication;
using eVoucher.BusinessLogicLayer;
using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
using eVoucher.Helpers.Logging;
using eVoucher.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eVoucher.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/eVoucher")] 
    public class eVoucherCMSController : BaseAPIController
    {
        private readonly iJWTAuthentication authentication;
        private readonly IConfiguration config;
        private readonly eVoucherDbContext _Dbcontext;
        private readonly IeVoucherCMSBusinessLogic businessLayer;
        private readonly ILogServices logging;
        public eVoucherCMSController(ILogServices log ,IeVoucherCMSBusinessLogic business, iJWTAuthentication auth, IConfiguration configuration, eVoucherDbContext Dbcontext)
        {
            logging = log;
            businessLayer = business;
            authentication = auth;
            config = configuration;
            _Dbcontext = Dbcontext;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("GetToken")]
        public async Task<IActionResult> GetToken([FromBody] AuthenticationAPIRequestModel requestModel)
        {
            string DecryptRequest = null;
            var getToken = new GetTokenResponseModel();
            string errMessage = null;
            try
            {
                DecryptRequest = eHelper.TripleDesDecryptor(requestModel.CredentialValue, config["TripleDesSecretKey"]);
                if (!string.IsNullOrEmpty(DecryptRequest))
                {
                    var getRequestObject = JsonConvert.DeserializeObject<AuthenticationModel>(DecryptRequest);
                    var GetUserID = await _Dbcontext.usersTableModel
                        .Where(user => user.MobileNo == getRequestObject.MobileNo && user.Password == getRequestObject.Password)
                        .FirstOrDefaultAsync();
                    if (GetUserID != null)
                    {
                        getToken.UserID = GetUserID.ID.ToString();
                        getToken.JwtToken = authentication.ValidateAndCreateJWT(getRequestObject);
                        getToken.ResponseCode = "000";
                        getToken.ResponseDescription = "Success";
                        return Ok(getToken);
                    }
                    else
                    {
                        return Unauthorized();
                    }                   
                }
                else
                {
                    return Unauthorized();
                }

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
                    RequestData = DecryptRequest,
                    ResponseData = JsonConvert.SerializeObject(getToken),
                    CreatedDate = DateTime.Now,
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success": errMessage

                }) ;
            }
        }


        [HttpPost]
        [Route("CMS/CreateEVoucher")]
        public async Task<IActionResult> CreateEVoucher([FromBody] eVoucherCreateAndUpdateRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var CreateVoucher = await businessLayer.CreateEVoucher(requestModel);
                respData = JsonConvert.SerializeObject(CreateVoucher);
                return Ok(CreateVoucher);

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
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("CMS/UpdateEVoucher")]
        public async Task<IActionResult> UpdateEVoucher([FromBody] eVoucherCreateAndUpdateRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var UpdatesVoucher = await businessLayer.UpdateEVoucher(requestModel);
                respData = JsonConvert.SerializeObject(UpdatesVoucher);
                return Ok(UpdatesVoucher);

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
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("CMS/DisplayEVoucherByID")]
        public async Task<IActionResult> DisplayEVoucherByID([FromBody] eVoucherDisplayByIDRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var displayResult = await businessLayer.GetEVoucherByID(requestModel);
                respData = JsonConvert.SerializeObject(displayResult);
                return Ok(displayResult);

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
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("CMS/DisplayListOfEVoucher")]
        public async Task<IActionResult> DisplayListOfEVoucher([FromBody] eVoucherDisplayByIDRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var ListResult = await businessLayer.GetListOfEVoucher(requestModel);
                respData = JsonConvert.SerializeObject(ListResult);
                return Ok(ListResult);

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
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

        [HttpPost]
        [Route("CMS/DeactivateEVoucherByID")]
        public async Task<IActionResult> DeactivateEVoucherByID([FromBody] eVoucherDeactivateRequestModel requestModel)
        {
            string respData = null;
            string errMessage = null;
            try
            {
                if (ValidateRequestModel(requestModel) == false)
                {
                    return FailValidationResponse();
                }
                var result = await businessLayer.DeactivateEVoucher(requestModel);
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
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }
    }
}
