using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
using eVoucher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer
{
    public class eVoucherCMSBusinessLogic: IeVoucherCMSBusinessLogic
    {
        private eVoucherDbContext _dbContext;
        private IConfiguration configuration;
        public eVoucherCMSBusinessLogic(eVoucherDbContext context, IConfiguration config)
        {
            configuration = config;
            _dbContext = context;
        }
        #region CMS
        public async Task<APIResponseModel> CreateEVoucher(eVoucherCreateRequestModel requestModel)
        {
            var ValidateEVoucher = await validateCreateToken(requestModel);
            if (!string.IsNullOrEmpty(ValidateEVoucher))
            {
                return new APIResponseModel {ResponseCode ="012", ResponseDescription = ValidateEVoucher };
            }

            var InternalImageUrl ="eVoucher_" + Guid.NewGuid()+eVoucherHelpers.GetFileExtension(requestModel.Base64Image.Substring(0, 5));

            Base64ToImage(requestModel.Base64Image).Save(configuration["ImageURL"] + InternalImageUrl);

            await _dbContext.AddAsync(new eVoucherTableModel
            {
                Title = requestModel.Title,
                Description = requestModel.Description,
                ExpireDate = DateTime.Parse(requestModel.ExpireDate),
                ImageURL = InternalImageUrl,
                Amount = decimal.Parse(requestModel.Amount),
                PaymentMethodID = requestModel.PaymentMethodID == null ? 0 : int.Parse(requestModel.PaymentMethodID),
                Quantity = int.Parse(requestModel.Quantity),
                BuyType = requestModel.BuyType,
                RedeemPerUser = int.Parse(requestModel.RedeemPerUser),
                CreatedDate = DateTime.Now,
                CreatedBy = int.Parse(requestModel.UserID),
                eStatus = 1
            }) ;
            await  _dbContext.SaveChangesAsync();
            return new APIResponseModel { ResponseCode = "000", ResponseDescription = "Success" }; ;
        }
        public async Task<List<eVoucherTableModel>> getListOfToken()
        {
            var lstOfValues = await (from d in _dbContext.eVoucher orderby d.ID select d).ToListAsync();
            return lstOfValues;
        }
        #endregion

        #region Validations
        public async Task<string> validateCreateToken(eVoucherCreateRequestModel requestModel)
        {
            var isExistVoucher =await _dbContext.eVoucher.Where(voucher => voucher.Amount == decimal.Parse(requestModel.Amount) && voucher.Title == requestModel.Title).Select(voucher => voucher.Title).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(isExistVoucher))
            {
                return "eVoucher with same Title and Amount already exist.";
            }
            if(int.Parse(requestModel.Quantity) < int.Parse(requestModel.RedeemPerUser))
            {
                return "Quantity cannot be greater than Redeem per user";
            }
            if(DateTime.Parse(requestModel.ExpireDate) < DateTime.Now)
            {
                return "Expire Date cannot be less than current date";
            }
            if (!string.IsNullOrEmpty(requestModel.PaymentMethodID))
            {
                var CheckPaymentIDExist = await _dbContext.paymentMethod.Where(paymentMethod => paymentMethod.ID == int.Parse(requestModel.PaymentMethodID)).FirstOrDefaultAsync();
                if(CheckPaymentIDExist == null)
                {
                    return "Invalid Payment method.";
                }
            }
            return null;
        }
     
        public System.Drawing.Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }
        #endregion
    }
}
