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
        public async Task<APIResponseModel> CreateEVoucher(eVoucherCreateAndUpdateRequestModel requestModel)
        {
            #region Admin Validation
            if(!await CheckAdminAuthorization(requestModel.UserID))
            {
                return new APIResponseModel { ResponseCode = "012", ResponseDescription = "Invalid Access" };
            }
            #endregion

            var ValidateEVoucher = await validateEVoucher(requestModel);
            if (string.IsNullOrEmpty(requestModel.PaymentMethodID)) { requestModel.PaymentMethodID = "0"; }
            if (!string.IsNullOrEmpty(ValidateEVoucher))
            {
                return new APIResponseModel {ResponseCode ="012", ResponseDescription = ValidateEVoucher };
            }

            var InternalImageUrl ="Images/eVoucher_" + Guid.NewGuid()+eHelper.GetFileExtension(requestModel.Base64Image.Substring(0, 5));            
            Base64ToImage(requestModel.Base64Image).Save(configuration["ImageURL"] + InternalImageUrl);

           
            await _dbContext.AddAsync(new eVoucherTableModel
            {
                Title = requestModel.Title,
                Description = requestModel.Description,
                ExpireDate = DateTime.Parse(requestModel.ExpireDate),
                ImageURL = InternalImageUrl,
                Amount = decimal.Parse(requestModel.Amount),
                PaymentMethodID =int.Parse(requestModel.PaymentMethodID) ,
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
        public async Task<APIResponseModel> UpdateEVoucher(eVoucherCreateAndUpdateRequestModel requestModel)
        {
            #region Validations
            #region Admin Validation
            if (!await CheckAdminAuthorization(requestModel.UserID))
            {
                return new APIResponseModel { ResponseCode = "012", ResponseDescription = "Invalid Access" };
            }
            #endregion
            if (string.IsNullOrEmpty(requestModel.ID ) || string.IsNullOrEmpty(requestModel.eStatus))
            {
                return new APIResponseModel { ResponseCode = "015", ResponseDescription = "Missing mandatory data" };
            }
            var ValidateEVoucher = await validateEVoucher(requestModel);
           
            if (!string.IsNullOrEmpty(ValidateEVoucher))
            {
                return new APIResponseModel { ResponseCode = "016", ResponseDescription = ValidateEVoucher };
            }
            #endregion

            var getEVoucherbyID = await _dbContext.eVoucher.Where(voucher => voucher.ID == int.Parse(requestModel.ID)).Select(voucher => voucher).FirstOrDefaultAsync();
            if(getEVoucherbyID == null)
            {
                string ez = requestModel.ID;
                return new APIResponseModel { ResponseCode = "017", ResponseDescription = "Data not found" };
            }
            string InternalImageUrl = null;
            if (!string.IsNullOrEmpty(requestModel.Base64Image))
            {              
                DeleteImage(getEVoucherbyID.ImageURL);                
                InternalImageUrl = "Images/eVoucher_" 
                    + Guid.NewGuid() 
                    + eHelper.GetFileExtension(requestModel.Base64Image.Substring(0, 5));
                Base64ToImage(requestModel.Base64Image).Save(configuration["ImageURL"] + InternalImageUrl);
            }
            getEVoucherbyID.Title = requestModel.Title;
            getEVoucherbyID.Description = requestModel.Description;
            getEVoucherbyID.ExpireDate =DateTime.Parse(requestModel.ExpireDate);
            getEVoucherbyID.ImageURL = InternalImageUrl == null? getEVoucherbyID.ImageURL: InternalImageUrl;
            getEVoucherbyID.Amount =decimal.Parse(requestModel.Amount);
            getEVoucherbyID.PaymentMethodID = requestModel.PaymentMethodID == null ?0: int.Parse(requestModel.PaymentMethodID);
            getEVoucherbyID.Quantity =int.Parse(requestModel.Quantity);
            getEVoucherbyID.BuyType = requestModel.BuyType;
            getEVoucherbyID.RedeemPerUser = int.Parse(requestModel.RedeemPerUser);
            getEVoucherbyID.eStatus = int.Parse(requestModel.eStatus);
            getEVoucherbyID.UpdatedBy = requestModel.UserID;
            getEVoucherbyID.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _dbContext.SaveChangesAsync();

            return new APIResponseModel { ResponseCode = "000", ResponseDescription = "Success" }; ;
        }

        public async Task<APIResponseWithDataModel> GetEVoucherByID(eVoucherDisplayByIDRequestModel requestModel)
        {
            #region Admin Validation
            if (!await CheckAdminAuthorization(requestModel.UserID))
            {
                return new APIResponseWithDataModel { ResponseCode = "012", ResponseDescription = "Invalid Access" };
            }
            #endregion
            var getEVoucherbyID = await _dbContext.eVoucher.Where(voucher => voucher.ID == int.Parse(requestModel.ID)).Select(voucher => voucher).FirstOrDefaultAsync();
            return new APIResponseWithDataModel { ResponseCode = "000", ResponseDescription ="Success", data = getEVoucherbyID };
        }

        public async Task<APIResponseWithDataModel> GetListOfEVoucher(eVoucherDisplayByIDRequestModel requestModel)
        {
            #region Admin Validation
            if (!await CheckAdminAuthorization(requestModel.UserID))
            {
                return new APIResponseWithDataModel { ResponseCode = "012", ResponseDescription = "Invalid Access" };
            }
            #endregion
            var lstOfValues = await (from d in _dbContext.eVoucher orderby d.ID descending select d).ToListAsync();
            return new APIResponseWithDataModel { ResponseCode = "000", ResponseDescription = "Success", data = lstOfValues };
        }

        public async Task<APIResponseModel> DeactivateEVoucher(eVoucherDeactivateRequestModel requestModel)
        {         
            var getEVoucherbyID = await _dbContext.eVoucher.Where(voucher => voucher.ID == int.Parse(requestModel.ID)).Select(voucher => voucher).FirstOrDefaultAsync();
            if (getEVoucherbyID == null)
            {
                string ez = requestModel.ID;
                return new APIResponseModel { ResponseCode = "012", ResponseDescription = "Data not found" };
            }          
            getEVoucherbyID.eStatus = 0;
            getEVoucherbyID.UpdatedBy = requestModel.UserID;
            getEVoucherbyID.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _dbContext.SaveChangesAsync();

            return new APIResponseModel { ResponseCode = "000", ResponseDescription = "Success" }; ;
        }
        #endregion

        #region Validations
        public async Task<bool> CheckAdminAuthorization(string UserID)
        {
          var isAdmin = await(from US in _dbContext.usersTableModel
                            .Where(us => us.ID == int.Parse(UserID) && us.UserType == "Admin" && us.UserStatus == "1")
                            orderby US.ID descending select US).ToListAsync();
            if(isAdmin == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task<string> validateEVoucher(eVoucherCreateAndUpdateRequestModel requestModel)
        {
            if (string.IsNullOrEmpty(requestModel.ID)) { requestModel.ID = "0"; }
            var isExistVoucher =await _dbContext.eVoucher
                .Where(
                voucher => voucher.Amount == decimal.Parse(requestModel.Amount) && voucher.Title == requestModel.Title && voucher.ID != int.Parse(requestModel.ID))
                .Select(voucher => voucher.Title)
                .FirstOrDefaultAsync();
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
        public void DeleteImage(string ImageNameUrl)
        {
            var fileUrl = configuration["BaseFilePath"];
            string fullPath = fileUrl + ImageNameUrl;
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

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
