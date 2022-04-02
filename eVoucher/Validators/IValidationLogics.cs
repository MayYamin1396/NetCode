using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Validators
{
    public interface IValidationLogics
    {
        bool ChecknullRequestParameter<T>(T validateModel);
        string HasCorrectLength<T>(T validateModel);
    }
}
