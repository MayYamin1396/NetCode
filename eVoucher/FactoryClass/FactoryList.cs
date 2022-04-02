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
        public static ImodelSerializer ModelSerializerInitialization()
        {
            return new modelSerializer();
        }
        public static IValidationLogics ValidationLogicsInitialization()
        {
            return new ValidationLogics();
        }
     
    }
}
