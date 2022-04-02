using eVoucher.Authentication;
using eVoucher.BusinessLogicLayer;
using eVoucher.DataInfrastructure;
using eVoucher.Helpers.Logging;
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
        private readonly IeVoucherCMSBusinessLogic businessLayer;
        private readonly ILogServices logging;
        public eStoreController(ILogServices log, IeVoucherCMSBusinessLogic business, iJWTAuthentication auth, IConfiguration configuration, eVoucherDbContext Dbcontext)
        {
            logging = log;
            businessLayer = business;
            authentication = auth;
            config = configuration;
            _Dbcontext = Dbcontext;
        }

    }
}
