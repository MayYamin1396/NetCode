using eVoucher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Authentication
{
    public interface iJWTAuthentication
    {
        string ValidateAndCreateJWT(AuthenticationModel reqModel);
    }
}
