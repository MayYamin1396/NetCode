using eVoucher.Authentication;
using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
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

namespace eVoucher.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/CMS/eVoucher")] 
    public class eVoucherCMSController : BaseAPIController
    {
        iJWTAuthentication authentication;
        private readonly IConfiguration config;
        private readonly eVoucherDbContext _Dbcontext;
        public eVoucherCMSController(iJWTAuthentication auth, IConfiguration configuration, eVoucherDbContext Dbcontext)
        {          
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
            string getToken = null;
            string errMessage = null;
            try
            {
                DecryptRequest = TripleDesDecryptor(requestModel.CredentialValue, config["TripleDesSecretKey"]);
                if (!string.IsNullOrEmpty(DecryptRequest))
                {
                    var getRequestObject = JsonConvert.DeserializeObject<AuthenticationModel>(DecryptRequest);                  
                   
                    if (!string.IsNullOrEmpty(
                        _Dbcontext.usersTableModel
                        .Where(user => user.ID == getRequestObject.UserID && user.Password == getRequestObject.Password)
                        .Select(user => user.MobileNo).FirstOrDefault()))
                    {
                        getToken = authentication.ValidateAndCreateJWT(getRequestObject);
                    }
                    if (getToken == null)
                    {
                        return Unauthorized();
                    }
                    return Ok(getToken);
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
                Services<eVoucherLogTableModel>.LogServices(config).PerformLogging(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = DecryptRequest,
                    ResponseData = getToken,
                    CreatedDate = DateTime.Now,
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success": errMessage

                }) ;
            }
        }


        [HttpPost]
        [Route("CreateEVoucher")]
        public async Task<IActionResult> CreateEVoucher([FromBody] eVoucherCreateRequestModel requestModel)
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
                return Ok("");

            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return BadRequest();
            }
            finally
            {
                Services<eVoucherLogTableModel>.LogServices(config).PerformLogging(new eVoucherLogTableModel
                {
                    Method = Request.Method,
                    Route = Request.Path,
                    RequestData = DecryptRequest,
                    ResponseData = getToken,
                    CreatedDate = DateTime.Now,
                    Request_UserID = 0,
                    Message = errMessage == null ? "Success" : errMessage

                });
            }
        }

    }
}
