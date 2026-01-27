using System;
using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; - Wrong namespace
// using System.Web.Mvc; - Removed for .NET 8
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Represents filter attribute to validate customer password expiration
    /// </summary>
    public class ValidatePasswordAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes
        /// </summary>
        /// <param name="filterContext">The filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;


            //don't apply filter to child methods - removed (child actions don't exist)

            var actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor).ActionName;
            if (string.IsNullOrEmpty(actionName) || actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase))
                return;

            var controllerName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor).ControllerName;
            if (string.IsNullOrEmpty(controllerName) || controllerName.Equals("Customer", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //get current customer
            var customer = EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer;

            //check password expiration
            if (customer.PasswordIsExpired())
            {
                // TODO: Use IUrlHelperFactory to create URL helper properly
                var changePasswordUrl = "/Customer/ChangePassword"; // Hardcoded for now
                filterContext.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(changePasswordUrl);
            }
        }
    }
}

