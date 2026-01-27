using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
// using System.Web.Mvc; - Removed for .NET 8
using Nop.Core.Infrastructure;
using Nop.Services.Security;

namespace Nop.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly bool _dontValidate;


        public AdminAuthorizeAttribute()
            : this(false)
        {
        }

        public AdminAuthorizeAttribute(bool dontValidate)
        {
            this._dontValidate = dontValidate;
        }

        private void HandleUnauthorizedRequest(AuthorizationFilterContext filterContext)
        {
            filterContext.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
        }

        private IEnumerable<AdminAuthorizeAttribute> GetAdminAuthorizeAttributes(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor descriptor)
        {
            // ASP.NET Core uses different attribute discovery
            // Return this instance for now
            return new[] { this };
        }

        private bool IsAdminPageRequested(AuthorizationFilterContext filterContext)
        {
            var adminAttributes = GetAdminAuthorizeAttributes(filterContext.ActionDescriptor);
            if (adminAttributes != null && adminAttributes.Any())
                return true;
            return false;
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            // OutputCacheAttribute/* .IsChildAction removed */CacheActive removed - not applicable in ASP.NET Core

            if (IsAdminPageRequested(filterContext))
            {
                if (!this.HasAdminAccess())
                    this.HandleUnauthorizedRequest(filterContext);
            }
        }

        public virtual bool HasAdminAccess()
        {
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            bool result = permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel);
            return result;
        }
    }
}
