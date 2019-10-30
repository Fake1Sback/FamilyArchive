using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace FamilyArchive.Middleware
{
    public class WhitelistMiddleware
    {
        private RequestDelegate _next;
        private IConfiguration _config;

        public WhitelistMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool Permission = false;
            IConfigurationSection configurationSection = _config.GetSection("Whitelist");
            IEnumerable<KeyValuePair<string, string>> whiteList = configurationSection.AsEnumerable();
  
            var RemoteIp = context.Connection.RemoteIpAddress.MapToIPv4().GetAddressBytes();

            foreach (var a in whiteList)
            {
                IPAddress testAdress;
                bool ParseResult = IPAddress.TryParse(a.Value, out testAdress);

                if(ParseResult)
                {
                    byte[] testBytes = testAdress.GetAddressBytes();
                    if(testBytes.SequenceEqual(RemoteIp))
                    {
                        Permission = true;
                        break;
                    }
                }
            }

            if (Permission)
                await _next.Invoke(context);
            else
            {
                context.Response.StatusCode = 403;             
            }
        }
    }
}
