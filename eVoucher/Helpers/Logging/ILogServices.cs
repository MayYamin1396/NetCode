using eVoucher.DataInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Helpers.Logging
{
    public interface ILogServices
    {
        Task PerformLoggingAsync(eVoucherLogTableModel reqData);
    }
}
