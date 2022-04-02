using eVoucher.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Model
{
    public class RequestAndResponseModel
    {
    }
    public class requestModelForTesting : ValidateThisModel
    {
        [Required]
        [LengthValidation(10)]
        public string UserID { get; set; }
        [Required]
        [LengthValidation(10, 20)]
        public string SessionID { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExtraField { get; set; }
    }
    public class dollarpricehistoryResponseData
    {

        public int ID { get; set; }
        public int SellPrice { get; set; }

        public int BuyPrice { get; set; }

        public string DateAndTime { get; set; }
    }
    public class APIResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }
    public class AuthenticationModel
    {
        public int UserID { get; set; }
        public string Password { get; set; }
    }
    public class AuthenticationAPIRequestModel
    {
        public string CredentialValue { get; set; }
    }
    [Serializable]
    public class errorResponseModel
    {
        public errorResponseModel()
        {

        }
        public errorResponseModel(string respCode, string respDescription)
        {
            ResponseCode = respCode;
            ResponseDescription = respDescription;
        }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }

    }
}
