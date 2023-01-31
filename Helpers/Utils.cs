using System.Text.Json;
using Serilog;
using WebServicesEnrollment.Models;
namespace WebServicesEnrollment.Helpers
{
    public static class Utils
    {        
        public static void ImprimirLog(int responseCode, string message, string typeLog, AppLog appLog)
        {
            appLog.ResponseCode = responseCode;
            appLog.Message = message;
            appLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff")) - appLog.ResponseTime;
            if (typeLog.Equals("Information"))
            {
                appLog.Level = 20;
                Log.Information(JsonSerializer.Serialize(appLog));
            }
            else if (typeLog.Equals("Error"))
            {
                appLog.Level = 40;
                Log.Error(JsonSerializer.Serialize(appLog));
            }
            else if (typeLog.Equals("Debug"))
            {
                appLog.Level = 10;
                Log.Debug(JsonSerializer.Serialize(appLog));
            }
        }
    }
}