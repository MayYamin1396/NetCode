using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
using eVoucher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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
        #region estore Business
        public async Task<eShopSelfVoucherDetailModel> eVoucherDetail(int voucherID,int UserID)
        {
            #region Validate User
            if (! await isValidUser(UserID))
            {
                throw new AuthenticationException("101");
            }
            #endregion

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
        public async Task<List<eShopSelfVoucherListModel>> DisplayListOfActiveeVoucherByType(string BuyType,int UserID)
        {
            #region Validate User
            if (!await isValidUser(UserID))
            {
                throw new AuthenticationException("101");
            }
            #endregion
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
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion

            var calcualtionModel = new eShopCheckOutAmountCalculationModel();

            #region Validations

            var activeEVoucher = await (
                from Pmethod in _dbContext.eVoucher
                .Where(vc => vc.ID == int.Parse(requestModel.VoucherID) && vc.eStatus == 1)
                select Pmethod).FirstOrDefaultAsync();            
            
            if(activeEVoucher == null)
            {
                return InvalidResponse("014", "This eVoucher is deactivated.");
            }
            var GetUserIDByMobileNo = await (
              from users in _dbContext.usersTableModel
              .Where(us => us.MobileNo == requestModel.MobileNo)
              select users).FirstOrDefaultAsync();
            if (GetUserIDByMobileNo == null)
            {                
                return InvalidResponse("015", "Sorry, user does not exist.");
            }

            if (activeEVoucher.BuyType == "Self")
            {               
                if(GetUserIDByMobileNo.ID != int.Parse(requestModel.UserID))
                {
                    return InvalidResponse("016", "This Voucher cannot be gifted to other users.");
                }
            }
            else
            {              
                if (GetUserIDByMobileNo.ID == int.Parse(requestModel.UserID))
                {
                    return InvalidResponse("018", "TThis Voucher is for gifting.");                 
                }
            }
            if(activeEVoucher.Quantity < int.Parse(requestModel.Quantity))
            {
                return InvalidResponse("019", "You have entered more than available quantity.");
            }
            if (int.Parse(requestModel.Quantity) > 1000)
            {
                return InvalidResponse("020", "Our system only support maximum of 1000 promo code stack per transaction.");
            }
            if (int.Parse(requestModel.Quantity) > activeEVoucher.RedeemPerUser)
            {
                return InvalidResponse("021", "You have reached redeemed per user limit.");
            }
            else
            {
                var GetUserPurchasedCount = await (
               from UOV in _dbContext.usersOrderedVouchers
               .Where(uov => uov.eVoucherID == int.Parse(requestModel.VoucherID))
               select UOV).CountAsync();
                if((GetUserPurchasedCount + int.Parse(requestModel.Quantity)) > activeEVoucher.RedeemPerUser)
                {
                    return InvalidResponse("022", "You have reached redeemed per user limit.");
                }
            }
            var checkQuantity = await CheckSufficientQuantity(requestModel.VoucherID, int.Parse(requestModel.Quantity));
            if (!checkQuantity)
            {
                return InvalidResponse("023", "Insufficient Quantity");             
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
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion
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
                    && vt.ReceiverUserID == calculatedResult.ReceiverUserID
                    && vt.isValidated == "0")
                    select validate).FirstOrDefaultAsync();

            if(isTransactionValid == null)
            {
                return new eShopTransactionResponseModel { ResponseCode = "101", ResponseDescription = "Invalid Transaction",TransactionStatus = "Invalid" };
            }
            else
            {
                isTransactionValid.isValidated = "1";
                await _dbContext.SaveChangesAsync();

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
                else
                {
                    return new eShopTransactionResponseModel { ResponseCode = "090", ResponseDescription = "Something went wrong during transaction", TransactionStatus = "Failed" };
                }
            }
            return new eShopTransactionResponseModel { ResponseCode = "000", ResponseDescription = "Success, check transaction status in history in a few mins", TransactionStatus = "Pending" };
        }
        public async Task<PromoCodeResponseModel> eShopValidatePromocode(checkQRModel requestModel)
        {
            requestModel.data = eHelper.TripleDesDecryptor(requestModel.data, configuration["TripleDesSecretKey"]);
            var promoModel = JsonConvert.DeserializeObject<QRPromoCodeModel>(requestModel.data);
             var getPromoCodeStatus = await (
                from EV in _dbContext.eVoucher
               .Where(ev => ev.ID == int.Parse(promoModel.VoucherID)) 
                from UOV in _dbContext.usersOrderedVouchers
               .Where(uov => uov.eVoucherID == EV.ID)
               .DefaultIfEmpty()
                select new PromoCodeStatusModel
                {
                    PromoCode = promoModel.PromoCode,
                    Amount = EV.Amount.ToString(),
                    TransactionID = promoModel.TransactionID,
                    PromoCodeStatus =
                     (
                         UOV.PromoStatus == "1" ? "Valid" : "Used"
                     ),
                    Owner = (from users in _dbContext.usersTableModel
                            .Where(us => us.ID == UOV.UserID)
                             select users.FullName).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return new PromoCodeResponseModel { ResponseCode = "000", ResponseDescription = "Success",data = getPromoCodeStatus };
        }

        public async Task<PromoCodeResponseModel> eShopApplyPromocode(ApplyPromoCodeModel requestModel)
        {
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion
            var chargesCalculationModel = new ApplyPromoCodeResponseModel();

            chargesCalculationModel.Amount = 100000;
            chargesCalculationModel.Charges = 5000;
            chargesCalculationModel.Discount = 2000;
            chargesCalculationModel.PromoDiscount = 0;

            if (!string.IsNullOrEmpty(requestModel.PromoCode))
            {
                var getDiscount = await (
                from PmCode in _dbContext.usersOrderedVouchers
                .Where(pm => pm.Promocode == requestModel.PromoCode && pm.PromoStatus == "1")
                from EV in _dbContext.eVoucher
                .Where(ev => ev.ID == PmCode.eVoucherID)
                select new 
                {
                    UserID= PmCode.UserID,
                    PromoAmount = PmCode.PromoAmount,
                    ExpiredDate = EV.ExpireDate
                }).FirstOrDefaultAsync();
                if (getDiscount != null)
                {
                    if (getDiscount.ExpiredDate < DateTime.Now)
                    {
                        return new PromoCodeResponseModel { ResponseCode = "054", ResponseDescription = "Promocode is Expired" };
                    }
                    if (getDiscount.UserID != int.Parse(requestModel.UserID))
                    {
                        return new PromoCodeResponseModel { ResponseCode = "055", ResponseDescription = "Promocode is not elligible to apply by this User." };
                    }

                    chargesCalculationModel.PromoDiscount = getDiscount.PromoAmount;

                    var UpdatePromoStatus = await (
                      from PmCode in _dbContext.usersOrderedVouchers
                      .Where(pm => pm.Promocode == requestModel.PromoCode)
                      select PmCode).FirstOrDefaultAsync();
                    UpdatePromoStatus.PromoStatus = "2";
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    return new PromoCodeResponseModel { ResponseCode = "056", ResponseDescription = "Invalid Promo Code"};
                }
            }
            chargesCalculationModel.TotalAmount = 
                (chargesCalculationModel.Amount + chargesCalculationModel.Charges)
                - (chargesCalculationModel.PromoDiscount + chargesCalculationModel.Discount);
            return new PromoCodeResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = chargesCalculationModel };
        }

        public async Task<PromoCodeResponseModel> eShopPurchaseHistory(TransactionHistoryModel requestModel)
        {
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion
            var getTransactionHistory = await (
                from TH in _dbContext.transactionHistory
                .Where(th => th.SenderUserID == requestModel.UserID)
                orderby TH.ID descending
                select TH).ToListAsync();

            return new PromoCodeResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = getTransactionHistory };
        }
        public async Task<PromoCodeResponseModel> eShopPurchaseHistoryDetail(TransactionHistoryDetailModel requestModel)
        {
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion
            var getTransactionHistory = await (
                from UOV in _dbContext.usersOrderedVouchers
                .Where(uov => uov.TransactionID == requestModel.TransactionID)
                orderby UOV.ID descending
                from EV in _dbContext.eVoucher
                .Where(ev => ev.ID == UOV.eVoucherID)
                select new TransactionDetailResponseModel 
                {
                    QrCodeURL = UOV.PromoStatus == "2"?null : UOV.QrCodeURL,
                    Promocode = UOV.Promocode,
                    PromoAmount = UOV.PromoAmount.ToString(),
                    PromoStatus = 
                    (
                        (UOV.PromoStatus == "1" && EV.ExpireDate > DateTime.Now) ? "Valid":
                        UOV.PromoStatus == "2"? "Used":"Expired"                   
                    )

                }).ToListAsync();

            return new PromoCodeResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = getTransactionHistory };
        }
        public async Task<PromoCodeResponseModel> eShopGetPaymentMethodList(TransactionHistoryModel requestModel)
        {
            #region Validate User
            if (!await isValidUser(int.Parse(requestModel.UserID)))
            {
                throw new AuthenticationException("101");
            }
            #endregion
            var GetPaymentList = await (
                  from Pmethod in _dbContext.paymentMethod
                  .Where(pm => pm.MethodStatus == 1)
                  orderby Pmethod.Method ascending
                  select new 
                  { 
                        PaymentMethodName = Pmethod.Method,
                        PaymentID = Pmethod.ID
                  }).ToListAsync();

            return new PromoCodeResponseModel { ResponseCode = "000", ResponseDescription = "Success", data = GetPaymentList };
        }
        #endregion

        #region eStore Helper
        public eShopCheckOutResponseModel InvalidResponse(string code, string desc)
        {
            return new eShopCheckOutResponseModel { ResponseCode = code, ResponseDescription = desc };
        }
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
                    ReceiverUserID = requestModel.ReceiverUserID,
                    isValidated = "0"
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
                    isValidateExist.isValidated = validationData.isValidated;
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

        public async Task<bool> isValidUser(int UserID)
        {
            var checkUser = await (
               from US in _dbContext.usersTableModel
               .Where(us => us.ID == UserID && us.UserStatus == "1")
               select US).FirstOrDefaultAsync();
            if (checkUser != null)
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
