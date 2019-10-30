using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace FamilyArchive
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;

            var pathToContentRoot = Path.GetDirectoryName(pathToExe);


            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var host = WebHost.CreateDefaultBuilder(args).UseConfiguration(configuration).UseKestrel(options =>
            {
                string hostIp = configuration["HostIp"];
                if (hostIp == "*")
                {
                    options.ListenAnyIP(Convert.ToInt32(configuration["HttpPort"]));
                    options.ListenAnyIP(Convert.ToInt32(configuration["HttpsPort"]), listenOptions =>
                    {
                        listenOptions.UseHttps(configuration["CertName"], configuration["CertPassword"]);
                    });
                }
                else
                {
                    IPAddress address = new IPAddress(ASCIIEncoding.ASCII.GetBytes(configuration["HostIp"]));

                    options.Listen(address, Convert.ToInt32(configuration["HttpPort"]));
                    options.Listen(address, Convert.ToInt32(configuration["HttpsPort"]), listenOptions =>
                      {
                          listenOptions.UseHttps(configuration["CertName"], configuration["CertPassword"]);
                      });
                }

                options.Limits.MaxRequestBodySize = Convert.ToInt32(configuration["MaxRequestBodySize"]);
            }).UseStartup<Startup>().Build();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
