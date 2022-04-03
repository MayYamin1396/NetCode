using eVoucher.FactoryClass;
using eVoucher.Model;
using eVoucher.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace eVoucher.Controllers
{
    [ApiController]
    public class BaseAPIController : ControllerBase
    {

        public IValidationLogics Validation { get; set; }
        public ImodelSerializer Serializer { get; set; }
        public DateTime RequestedDateTime { get; set; }
        public string RequestPath { get; set; }
        public string ErrorMessage { get; set; }
        public errorResponseModel respModel { get; set; }


        public BaseAPIController()
        {
            Serializer = FactoryList.ModelSerializerInjection();
            Validation = FactoryList.ValidationLogicsInjection();
            ErrorMessage = null;
            respModel = new errorResponseModel();
            RequestedDateTime = DateTime.Now;
            if (Request != null)
            {
                PreLoadBaseAPIData();
            }
        }

        private void PreLoadBaseAPIData()
        {

            RequestPath = Request.Path;

        }
        protected bool ValidateRequestModel<T>(T requestModel)
        {
            /// check if request model should be validated
            if (CheckValidationForThisModel(requestModel))
            {
                #region CheckNull

                /// check if request model is null or not if null return false
                if (Validation.ChecknullRequestParameter(requestModel) == false)
                {
                    /// fetch all the validation failure from ModelState and serialize it 
                    FailValidationResponse("004", SerializeModelState(ModelState));
                    return false;
                }
                #endregion

                #region LengthValidation 

                /// length validation of request parameter if failure string value will be returned
                string validaitonResult = Validation.HasCorrectLength(requestModel);
                if (!string.IsNullOrEmpty(validaitonResult))
                {
                    FailValidationResponse("004", validaitonResult);
                    return false;
                }

                #endregion
            }
            return true;
        }
        protected IActionResult FailValidationResponse()
        {
            if (respModel.ResponseCode != "000")
            {
                ErrorMessage = ErrorMessage ?? respModel.ResponseDescription;
            }

            return Ok(Serializer.SerializeModel(respModel));
        }
        protected IActionResult FailValidationResponse(string RespCode, string RespDesc)
        {
            respModel.ResponseCode = RespCode;
            respModel.ResponseDescription = RespDesc.Replace("[", "").Replace("]","").Replace("\"", "").Replace(",", "");
            return Ok(new errorResponseModel(RespCode, RespDesc));
        }
        protected string SerializeModelState(ModelStateDictionary msd)
        {
            var errorMessages = new List<string>();

            if (msd.IsValid)
            {
                return string.Empty;
            }

            foreach (var keyModelStatePair in msd)
            {
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    errorMessages.AddRange(errors.Select(x => x.ErrorMessage));
                }
            }

            return JsonConvert.SerializeObject(errorMessages);
        }
        public bool CheckValidationForThisModel<T>(T shouldValidate)
        {
            return shouldValidate is ValidateThisModel;
        }
        public static string TripleDesDecryptor(string encodedText, string key)
        {
            TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();

            byte[] byteBuff;

            try
            {
                desCryptoProvider.Key = Encoding.UTF8.GetBytes(key);
                desCryptoProvider.IV = UTF8Encoding.UTF8.GetBytes("ABCDEFGH");
                byteBuff = Convert.FromBase64String(encodedText);

                string plaintext = Encoding.UTF8.GetString(desCryptoProvider.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                return plaintext;
            }
            catch (Exception except)
            {
                Console.WriteLine(except + "\n\n" + except.StackTrace);
                return null;
            }
        }
    }
}
