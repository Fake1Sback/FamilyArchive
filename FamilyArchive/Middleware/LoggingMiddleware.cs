using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace FamilyArchive.Middleware
{
    public class LoggingMiddleware
    {
        private RequestDelegate _next;
        private IHostingEnvironment _hostingEnvironment;
        private static readonly object _lockObj = new object();

        public LoggingMiddleware(RequestDelegate next, IHostingEnvironment hostingEnvironment)
        {
            _next = next;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string ProjectLogsDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "Logs");
            bool LogFolderExists = Directory.Exists(ProjectLogsDirectory);
            if (!LogFolderExists)
                _next(context);

            DateTime dateTimeNow = DateTime.Now;
            string CurrentMonthLogFolderName = $"{dateTimeNow.ToString("MM_yyyy")}_LOGS";

            string CurrentMonthDirectory = Path.Combine(ProjectLogsDirectory, CurrentMonthLogFolderName);

            if (!Directory.Exists(CurrentMonthDirectory))
            {
                Directory.CreateDirectory(CurrentMonthDirectory);
            }
                
            string CurrentDayLogName = $"{dateTimeNow.ToString("dd")}_Log.txt";
            string CurrentDayLogFileName = Path.Combine(CurrentMonthDirectory, CurrentDayLogName);              
            string RequestLog = $"{dateTimeNow.ToString("HH:mm:ss")} ClientIpv4:{context.Connection.RemoteIpAddress.MapToIPv4()} Request:{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
              
            await _next(context);

            RequestLog += $" ResponseStatusCode:{context.Response.StatusCode} ResponseContentType:{context.Response.ContentType}";
            if (context.Items.ContainsKey("Exception"))
                RequestLog += $" Exception:{context.Items["Exception"]}" + Environment.NewLine;
            else
                RequestLog += Environment.NewLine;
                
            lock (_lockObj)
            {
                File.AppendAllText(CurrentDayLogFileName, RequestLog);
            }
        }
    }
}
