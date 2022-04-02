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
    public class LogServices<IRequest> : ILogServices<IRequest>
    {
        private static string LoggingMode;
        private readonly IConfiguration config;
        private readonly eVoucherDbContext _Dbcontext;
        public LogServices(IConfiguration configuration)
        {
            config = configuration;         
            LoggingMode = config["TripleDesSecretKey"];
        }
        delegate void LoggingProcess(IRequest reqData);
        public void PerformLogging(IRequest reqData)
        {
            LoggingProcess LoggingProcessOption;
            IAsyncResult asynCall;
            switch (LoggingMode)
            {
                case "0":
                    break;

                case "1":
                    LoggingProcessOption = new LoggingProcess(LogInDataBase);
                    asynCall = LoggingProcessOption.BeginInvoke(reqData, null, null);
                    LoggingProcessOption.EndInvoke(asynCall);
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
            Tuple<IRequest> state = (Tuple<IRequest>)result.AsyncState;
            LogInFile(state.Item1);
        }
        private async void LogInDataBase(IRequest reqData)
        {
            try
            {             
                await _Dbcontext.AddAsync((eVoucherLogTableModel)Convert.ChangeType(reqData, typeof(eVoucherLogTableModel)));
                await _Dbcontext.SaveChangesAsync();
            }
            catch (Exception)
            {

            }          
        }

        private void LogInFile(IRequest reqData)
        {
            string errMsg = null;
            var RequetModel = (eVoucherLogTableModel)Convert.ChangeType(reqData, typeof(eVoucherLogTableModel));
            try
            {
                string LogMessage =  RequetModel.Message;
                string[] lines = new string[]{$@"
Request Data:  {RequetModel.RequestData} 
Response Data:  {RequetModel.ResponseData} 
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
