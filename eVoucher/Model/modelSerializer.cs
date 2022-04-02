using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public class modelSerializer : ImodelSerializer
    {
        public string SerializeModel(object reqObj)
        {
            var options = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() };

            return JsonConvert.SerializeObject(reqObj, options);
        }
    }
}
