using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
using eVoucher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer.eStore
{
    public class eStoreBusinessLogic: IeStoreBusinessLogic
    {
        private eVoucherDbContext _dbContext;
        private JobQueueLogic JQ;
        private IConfiguration configuration;
        public eStoreBusinessLogic(eVoucherDbContext context, IConfiguration config, JobQueueLogic jobqueue)
        {
            JQ = jobqueue;
            configuration = config;
            _dbContext = context;
        }
        public async Task<eShopSelfVoucherDetailModel> eVoucherDetail(int voucherID)
        {
            var getEVoucherbyID = await (
                from voucher in _dbContext.eVoucher
                .Where(vc => vc.ID == voucherID)
                orderby voucher.Title ascending
                from paymentMethod in _dbContext.paymentMethod 
                .Where(pm => pm.ID == voucher.PaymentMethodID )
                .DefaultIfEmpty()
                select new eShopSelfVoucherDetailModel
                {
                    ID = voucher.ID,
                    Title = voucher.Title,
                    Description = voucher.Description,
                    ExpireDate = voucher.ExpireDate,
                    ImageURL = voucher.ImageURL,
                    Amount = voucher.Amount,
                    PaymentMethod = paymentMethod.Method == null? "None": paymentMethod.Method,
                    Discount =
                    (
                        paymentMethod.DiscountType == "Fix" ? (paymentMethod.DiscountValue + " Ks"):
                        paymentMethod.DiscountType == "Pcent"? (paymentMethod.DiscountValue + "%") : "None"
                    ),
                    Stock = voucher.Quantity - (from orderCount in _dbContext.eVoucherQuantityControl
                                                .Where(x => x.VoucherID == voucher.ID.ToString())
                                                select orderCount.VoucherPurchasedQuantity).FirstOrDefault(),
                    BuyType = voucher.BuyType,
                    RedeemPerUser = voucher.RedeemPerUser
                }).FirstOrDefaultAsync();

            return getEVoucherbyID;
        }
        public async Task<List<eShopSelfVoucherListModel>> DisplayListOfActiveeVoucherByType(string BuyType)
        {
            var getEVoucherbyID = await (
                from voucher in _dbContext.eVoucher
                .Where(vc => (vc.Quantity - (from orderCount in _dbContext.eVoucherQuantityControl
                                                .Where(x => x.VoucherID == vc.ID.ToString())
                                                  select orderCount.VoucherPurchasedQuantity).FirstOrDefault()) > 0 
                                                  && vc.eStatus == 1
                                                  && vc.BuyType == BuyType)
                orderby voucher.Title ascending
                from paymentMethod in _dbContext.paymentMethod
                .Where(pm => pm.ID == voucher.PaymentMethodID)
                .DefaultIfEmpty()
                select new eShopSelfVoucherListModel
                {
                    ID = voucher.ID,
                    Title = voucher.Title,                 
                    ExpireDate = voucher.ExpireDate,
                    ImageURL = voucher.ImageURL,
                    Amount = voucher.Amount,
                    BuyType = voucher.BuyType
                }).ToListAsync();

            return getEVoucherbyID;
        }
        public async Task<eShopCheckOutResponseModel> eShopCheckOut(eShopCheckOutRequestModel requestModel)
        {
            var calcualtionModel = new eShopCheckOutAmountCalculationModel();

            #region Validations

            var activeEVoucher = await (
                from Pmethod in _dbContext.eVoucher
                .Where(vc => vc.ID == int.Parse(requestModel.VoucherID) && vc.eStatus == 1)
                select Pmethod).FirstOrDefaultAsync();            
            
            if(activeEVoucher == null)
            {
                return new eShopCheckOutResponseModel { ResponseCode = "014", ResponseDescription = "This eVoucher is deactivated." };
            }
            var GetUserIDByMobileNo = await (
              from users in _dbContext.usersTableModel
              .Where(us => us.MobileNo == requestModel.MobileNo)
              select users).FirstOrDefaultAsync();
            if (GetUserIDByMobileNo == null)
            {
                return new eShopCheckOutResponseModel { ResponseCode = "015", ResponseDescription = "Sorry, user does not exist." };
            }

            if (activeEVoucher.BuyType == "Self")
            {               
                if(GetUserIDByMobileNo.ID != int.Parse(requestModel.UserID))
                {
                    return new eShopCheckOutResponseModel { ResponseCode = "016", ResponseDescription = "This Voucher cannot be gifted to other users." };
                }
            }
            else
            {              
                if (GetUserIDByMobileNo.ID == int.Parse(requestModel.UserID))
                {
                    return new eShopCheckOutResponseModel { ResponseCode = "018", ResponseDescription = "This Voucher is for gifting." };
                }
            }
            if(activeEVoucher.Quantity < int.Parse(requestModel.Quantity))
            {
                return new eShopCheckOutResponseModel { ResponseCode = "019", ResponseDescription = "You have entered more than available quantity." };
            }
            var checkQuantity = await CheckSufficientQuantity(requestModel.VoucherID, int.Parse(requestModel.Quantity));
            if (!checkQuantity)
            {
                return new eShopCheckOutResponseModel { ResponseCode = "020", ResponseDescription = "Insufficient Quantity" };
            }

            #endregion

            var PreTotalAmount = int.Parse(requestModel.Quantity) * activeEVoucher.Amount;
            decimal DiscountValue = 0;          
            if (activeEVoucher.PaymentMethodID.ToString() == requestModel.PaymentMethodID)
            {
                DiscountValue = await DiscountAmountCalculation(requestModel.PaymentMethodID, PreTotalAmount);                
            }
            calcualtionModel.Amount = string.Format("{0:0.00}", activeEVoucher.Amount);
            calcualtionModel.Discount = string.Format("{0:0.00}",DiscountValue);
            calcualtionModel.TotalAmount = string.Format("{0:0.00}", (PreTotalAmount - DiscountValue));
            calcualtionModel.Quantity = requestModel.Quantity;
            calcualtionModel.VoucherID = requestModel.VoucherID;
            calcualtionModel.ReceiverUserID = GetUserIDByMobileNo.ID.ToString();
            if (await CreateOrUpdateTransactionValidation(calcualtionModel,int.Parse(requestModel.UserID)))
            {
                return new eShopCheckOutResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = calcualtionModel };
            }
            else
            {
                return new eShopCheckOutResponseModel { ResponseCode = "016", ResponseDescription = "Something went wrong" };
            }
        }

        public async Task<eShopTransactionResponseModel> eShopTransaction(eShopTransactionRequestModel requestModel)
        {         
            #region Validate Transaction
            var calculatedResult = requestModel.amountCalculationResult;
            var isTransactionValid = await (
                    from validate in _dbContext.ValidateTransaction
                    .Where(vt => 
                    vt.UserID == int.Parse(requestModel.UserID) 
                    && vt.OriginalAmount == calculatedResult.Amount
                    && vt.DiscountAmount == calculatedResult.Discount
                    && vt.TotalAmount == calculatedResult.TotalAmount
                    && vt.Quantity == calculatedResult.Quantity
                    && vt.VoucherID == calculatedResult.VoucherID
                    && vt.ReceiverUserID == calculatedResult.ReceiverUserID)
                    select validate).FirstOrDefaultAsync();

            if(isTransactionValid == null)
            {
                return new eShopTransactionResponseModel { ResponseCode = "101", ResponseDescription = "Invalid Transaction",TransactionStatus = "Invalid" };
            }
            #endregion
            var checkQuantity = await CheckSufficientQuantity(calculatedResult.VoucherID, int.Parse(calculatedResult.Quantity));
            if (!checkQuantity)
            {
                return new eShopTransactionResponseModel { ResponseCode = "020", ResponseDescription = "Insufficient Quantity", TransactionStatus = "Invalid" };
            }
            var preOrderUpdate =await UpdateVoucherControl(calculatedResult.VoucherID, int.Parse(calculatedResult.Quantity));
            if (preOrderUpdate)
            {
                var CreatePaymentByPaymentMethod = await CreateTransactionAsync(requestModel);
                if (!string.IsNullOrEmpty(CreatePaymentByPaymentMethod))
                {
                    calculatedResult.TransactionID = CreatePaymentByPaymentMethod;
                    for (int i = 0; i < int.Parse(calculatedResult.Quantity); i++)
                    {
                        JQ.EnqueuePromoJob(calculatedResult);
                    }
                    var UpdateTransactionStatus = await (
                    from TH in _dbContext.transactionHistory
                    .Where(th => th.TransactionID == calculatedResult.TransactionID)
                    select TH).FirstOrDefaultAsync();
                    UpdateTransactionStatus.TransactionStatus = "Success";
                    UpdateTransactionStatus.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await _dbContext.SaveChangesAsync();
                }
            }
            return new eShopTransactionResponseModel { ResponseCode = "000", ResponseDescription = "Success - Your transaction is pending, check in history in a few mins", TransactionStatus = "Pending" };
        }


        #region eStore Logics
        public async Task<decimal> DiscountAmountCalculation(string PaymentMethodID, decimal TotalAmount)
        {
            var getDiscount = await(
                from Pmethod in _dbContext.paymentMethod
                .Where(vc => vc.ID == int.Parse(PaymentMethodID) && vc.MethodStatus == 1)
                select Pmethod).FirstOrDefaultAsync();

            decimal discountValue = 0;
            string DiscountType = null;
            if (getDiscount != null)
            {
                DiscountType = getDiscount.DiscountType;
                discountValue = getDiscount.DiscountValue;

            }
            
            return GetDiscountAmount(DiscountType, TotalAmount, discountValue);

        }
        public static decimal GetDiscountAmount(string discountType, decimal amount, decimal discountvalue)
        {          
            decimal returnAmount = 0;
            if (discountType == "Pcent")
            {
                returnAmount = amount * (discountvalue / 100);
            }
            else if (discountType == "2")
            {
                returnAmount = discountvalue;
            }
            return returnAmount;
        }
        public async Task<bool> CreateOrUpdateTransactionValidation(eShopCheckOutAmountCalculationModel requestModel,int UserID)
        {
            try
            {
                var isValidateExist = await (
                    from validate in _dbContext.ValidateTransaction
                    .Where(vt => vt.UserID == UserID)
                    select validate).FirstOrDefaultAsync();

                var validationData = new ValidateTransactionTableModel
                {
                    UserID = UserID,
                    OriginalAmount = requestModel.Amount,
                    DiscountAmount = requestModel.Discount,
                    TotalAmount = requestModel.TotalAmount,
                    Quantity = requestModel.Quantity,
                    VoucherID = requestModel.VoucherID,
                    ReceiverUserID = requestModel.ReceiverUserID
                };

                if (isValidateExist != null)
                {
                    isValidateExist.UserID = validationData.UserID;
                    isValidateExist.OriginalAmount = validationData.OriginalAmount;
                    isValidateExist.DiscountAmount = validationData.DiscountAmount;
                    isValidateExist.TotalAmount = validationData.TotalAmount;
                    isValidateExist.Quantity = validationData.Quantity;
                    isValidateExist.VoucherID = validationData.VoucherID;
                    isValidateExist.ReceiverUserID = validationData.ReceiverUserID;
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    await _dbContext.AddAsync(validationData);
                    await _dbContext.SaveChangesAsync();
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
           
        }
        public async Task<string> CreateTransactionAsync(eShopTransactionRequestModel requestModel)
        {
            var TransactionID =eHelper.epochTime().ToString();
            switch (requestModel.PaymentMethodID)
            {
                case "1":
                    /// Transaction Method 1
                    break;

                case "2":
                    /// Transaction Method 2
                    break;

                case "3":
                    /// Transaction Method 3
                    break;

                case "4":
                    /// Transaction Method 4
                    break;
            }
            await _dbContext.AddAsync(new TransactionHistoryTableModel
            {
                TransactionID = TransactionID,
                SenderUserID = requestModel.UserID,
                ReceiverUserID = requestModel.amountCalculationResult.ReceiverUserID,
                TransactionStatus = "Pending",
                OriginalAmount = requestModel.amountCalculationResult.Amount,
                DiscountAmount = requestModel.amountCalculationResult.Discount,
                TotalAmount = requestModel.amountCalculationResult.TotalAmount,
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }) ;
            await _dbContext.SaveChangesAsync();
            return TransactionID;
        }
     
        public async Task<bool> UpdateVoucherControl(string VoucherID,int Quantity)
        {
            var VoucherControl = await (
               from EC in _dbContext.eVoucherQuantityControl
               .Where(ec => ec.VoucherID == VoucherID)
               select EC).FirstOrDefaultAsync();
            if (VoucherControl != null)
            {
                VoucherControl.VoucherPurchasedQuantity = VoucherControl.VoucherPurchasedQuantity + Quantity;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                await _dbContext.AddAsync(new eVoucherQuantityControlTableModel 
                {
                    VoucherPurchasedQuantity = Quantity,
                    VoucherID = VoucherID
                });
                await _dbContext.SaveChangesAsync();
            }
            return true;
        }
        public async Task<bool> CheckSufficientQuantity(string VoucherID, int Quantity)
        {
            var checkSufficientQuantity = await (
               from VC in _dbContext.eVoucher
               .Where(vc =>  vc.eStatus == 1 && vc.ID == int.Parse(VoucherID))
               select new checkRemainerModel { 
               remainer = VC.Quantity - (from orderCount in _dbContext.eVoucherQuantityControl
                                                .Where(x => x.VoucherID == VC.ID.ToString())
                                         select orderCount.VoucherPurchasedQuantity).FirstOrDefault()
               }).FirstOrDefaultAsync();
            if (checkSufficientQuantity != null && checkSufficientQuantity.remainer >= Quantity)
            {              
                return true;                
            }
            else
            {
                return false;
            }

        }
        #endregion
    }
}
