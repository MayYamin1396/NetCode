using eVoucher.DataInfrastructure;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Helpers.Logging
{
    public class LogServices : ILogServices
    {
        private static string LoggingMode;
        private readonly IConfiguration config;
        private readonly eVoucherDbContext _Dbcontext;
        public LogServices(IConfiguration configuration, eVoucherDbContext dbContext)
        {
            _Dbcontext = dbContext;
            config = configuration;         
            LoggingMode = config["LoggingMode"];
        }
        delegate void LoggingProcess(eVoucherLogTableModel reqData);
        public async Task PerformLoggingAsync(eVoucherLogTableModel reqData)
        {
            LoggingProcess LoggingProcessOption;
            IAsyncResult asynCall;
            switch (LoggingMode)
            {
                case "0":
                    break;

                case "1":
                    LoggingProcessOption = new LoggingProcess(LogInDataBase);
                    var begin = Task.Run(() => LoggingProcessOption.Invoke(reqData));
                    var followUpTask = begin.ContinueWith(null);
                    await begin;
                    await followUpTask;
                    break;

                case "2":
                    LoggingProcessOption = new LoggingProcess(LogInFile);
                    asynCall = LoggingProcessOption.BeginInvoke(reqData, null, null);
                    LoggingProcessOption.EndInvoke(asynCall);
                    break;

                case "12":
                    LoggingProcessOption = new LoggingProcess(LogInDataBase);
                    asynCall = LoggingProcessOption.BeginInvoke(reqData, callBack, Tuple.Create(reqData));
                    LoggingProcessOption.EndInvoke(asynCall);
                    break;
            }
        }
        private void callBack(IAsyncResult result)
        {
            Tuple<eVoucherLogTableModel> state = (Tuple<eVoucherLogTableModel>)result.AsyncState;
            LogInFile(state.Item1);
        }
        private async void LogInDataBase(eVoucherLogTableModel reqData)
        {
            try
            {             
                await _Dbcontext.AddAsync(reqData);
                await _Dbcontext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string exmsg = ex.Message;
            }          
        }

        private void LogInFile(eVoucherLogTableModel reqData)
        {          
            try
            {
                string LogMessage = reqData.Message;
                string[] lines = new string[]{$@"
Request Data:  {reqData.RequestData} 
Response Data:  {reqData.ResponseData} 
LogMessage: {LogMessage}
RequestDatetime:  {DateTime.Now}"
                };
                File.AppendAllLines(config["LoggingFilePath"], lines);
            }
            catch (Exception)
            {            
                
            }          
        }

    }
}
