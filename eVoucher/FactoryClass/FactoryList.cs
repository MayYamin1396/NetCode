using eVoucher.Model;
using eVoucher.Validators;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.FactoryClass
{
    public class FactoryList
    {
        public static ImodelSerializer ModelSerializerInjection()
        {
            return new modelSerializer();
        }
        public static IValidationLogics ValidationLogicsInjection()
        {
            return new ValidationLogics();
        }
     
    }
}
