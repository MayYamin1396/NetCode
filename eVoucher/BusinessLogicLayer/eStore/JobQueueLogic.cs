using eVoucher.DataInfrastructure;
using eVoucher.Helpers;
using eVoucher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eVoucher.BusinessLogicLayer.eStore
{
    public class JobQueueLogic
    {
        public BlockingCollection<eShopCheckOutAmountCalculationModel> _PromoJobs = new BlockingCollection<eShopCheckOutAmountCalculationModel>();
        private eVoucherDbContext _dbContext;
        private IConfiguration configuration;
        public JobQueueLogic(eVoucherDbContext context, IConfiguration config)
        {
            _dbContext = context;
            configuration = config;
            eHelper.InitializeThread(new Thread(new ThreadStart(JobStartPromo)));
        }
        public void EnqueuePromoJob(eShopCheckOutAmountCalculationModel job)
        {
            _PromoJobs.Add(job);
        }
        private async void JobStartPromo()
        {
            foreach (var job in _PromoJobs.GetConsumingEnumerable(CancellationToken.None))
            {
                try
                {
                    await CreatePromocode(job);
                }
                catch (Exception ex)
                {
                    string checkmyexception = ex.Message;
                }
            }
        }
        public async Task<bool> CreatePromocode(eShopCheckOutAmountCalculationModel requestModel)
        {
            int breaker = 0;
            bool breakerBroken = false;
            string PromoCode = GeneratePromoCode();
            bool create = false;
            var dbOption = new DbContextOptionsBuilder<eVoucherDbContext>()
                .UseMySQL(configuration.GetConnectionString("Default")).Options;
            using (_dbContext = new eVoucherDbContext(dbOption))
            {
                var isPromoExist = await (
                    from promo in _dbContext.usersOrderedVouchers
                    .Where(pm =>
                    pm.Promocode == PromoCode)
                    select promo).FirstOrDefaultAsync();

                while (isPromoExist != null)
                {
                    PromoCode = GeneratePromoCode();
                    isPromoExist = await (
                        from promo in _dbContext.usersOrderedVouchers
                        .Where(pm =>
                        pm.Promocode == PromoCode)
                        select promo).FirstOrDefaultAsync();
                    breaker++;
                    if (breaker == 20)
                    {
                        breakerBroken = true;
                        break;
                    }
                }
                if (breakerBroken)
                {
                    return false;
                }

                create = await CreateQR(new eShopQRModel
                {
                    PromoCode = PromoCode,
                    Amount = requestModel.Amount,
                    Discount = requestModel.Discount,
                    TotalAmount = requestModel.TotalAmount,
                    VoucherID = requestModel.VoucherID,
                    OwnerID = requestModel.ReceiverUserID,
                    TransactionID = requestModel.TransactionID
                });
            }
            if (create)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string GeneratePromoCode()
        {
            var num = "0123456789";
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            var charsresult = new string(
                Enumerable.Repeat(chars, 5)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var numresult = new string(
               Enumerable.Repeat(num, 6)
                         .Select(s => s[random.Next(s.Length)])
                         .ToArray());
            return  (numresult + charsresult);
        }
        public async Task<bool> CreateQR(eShopQRModel requestModel)
        {
            var QRInformation = Encrypt(JsonConvert.SerializeObject(requestModel), configuration["TripleDesSecretKey"]);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRInformation, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var coloring = Color.FromArgb(71, 170, 136);
            var logoPath = "Images/BaseQR/baseQRImage.png";
            Bitmap qrCodeImage = qrCode.GetGraphic(50, coloring, Color.White, (Bitmap)Bitmap.FromFile(logoPath), 30, 100, true);
            var InternalImageUrl = "Images/PromoQR/" + Guid.NewGuid() + ".png";
            if (!File.Exists(InternalImageUrl))
            {
                qrCodeImage.Save(InternalImageUrl, ImageFormat.Png);
            }
            await _dbContext.AddAsync(new UsersOrderedVouchersTableModel
            {
                eVoucherID = requestModel.VoucherID,
                QrCodeURL = InternalImageUrl,
                TransactionID = requestModel.TransactionID,
                Promocode = requestModel.PromoCode,
                UserID = requestModel.OwnerID
            });
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public static string Encrypt(string source, string key)
        {
            TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();

            byte[] byteBuff;

            try
            {
                desCryptoProvider.Key = Encoding.UTF8.GetBytes(key);
                desCryptoProvider.IV = UTF8Encoding.UTF8.GetBytes("ABCDEFGH");
                byteBuff = Encoding.UTF8.GetBytes(source);

                string iv = Convert.ToBase64String(desCryptoProvider.IV);
                Console.WriteLine("iv: {0}", iv);

                string encoded =
                    Convert.ToBase64String(desCryptoProvider.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));

                return encoded;
            }
            catch (Exception except)
            {
                Console.WriteLine(except + "\n\n" + except.StackTrace);
                return null;
            }
        }
    }
}
