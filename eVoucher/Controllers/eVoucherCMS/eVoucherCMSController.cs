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
                DecryptRequest = TripleDesDecryptor(requestModel.CredentialValue, config["TripleDesSecretKey"]);
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
                new Thread(() => logging.PerformLoggingAsync(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = DecryptRequest,
                    ResponseData = JsonConvert.SerializeObject(getToken),
                    CreatedDate = DateTime.Now,
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success": errMessage

                })).Start() ;
            }
        }


        [HttpPost]
        [Route("CMS/CreateEVoucher")]
        public async Task<IActionResult> CreateEVoucher([FromBody] eVoucherCreateAndUpdateRequestModel requestModel)
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
                var CreateVoucher = await businessLayer.CreateEVoucher(requestModel);
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
        [Route("CMS/UpdateEVoucher")]
        public async Task<IActionResult> UpdateEVoucher([FromBody] eVoucherCreateAndUpdateRequestModel requestModel)
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
                var CreateVoucher = await businessLayer.UpdateEVoucher(requestModel);
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
        [Route("CMS/DisplayEVoucherByID")]
        public async Task<IActionResult> DisplayEVoucherByID([FromBody] eVoucherDisplayByIDRequestModel requestModel)
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
                var CreateVoucher = await businessLayer.GetEVoucherByID(requestModel);
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
        [Route("CMS/DisplayListOfEVoucher")]
        public async Task<IActionResult> DisplayListOfEVoucher([FromBody] eVoucherDisplayByIDRequestModel requestModel)
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
                var CreateVoucher = await businessLayer.GetListOfEVoucher();
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
        [Route("CMS/DeactivateEVoucherByID")]
        public async Task<IActionResult> DeactivateEVoucherByID([FromBody] eVoucherDeactivateRequestModel requestModel)
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
                var CreateVoucher = await businessLayer.DeactivateEVoucher(requestModel);
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
