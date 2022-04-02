using eVoucher.DataInfrastructure;
using eVoucher.Helpers.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Helpers
{
    public class Services<IRequest>
    {
        public static ILogServices<IRequest> LogServices(IConfiguration config, eVoucherDbContext context)
        {
            return new LogServices<IRequest>(config, context);
        }
    }
}
