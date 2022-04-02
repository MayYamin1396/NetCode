using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public interface ImodelSerializer
    {
        string SerializeModel(object reqObj);
    }
    public interface ValidateThisModel
    {

    }
}
