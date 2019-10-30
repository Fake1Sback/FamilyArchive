using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilyArchive.Filters
{
    public class AllExceptionsFilter : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new ContentResult
            {
                Content = context.Exception.Message
            };

            context.HttpContext.Items.Add("Exception", context.Exception.Message);
            context.HttpContext.Response.StatusCode = 400;
            context.ExceptionHandled = true;          
        }
    }
}
