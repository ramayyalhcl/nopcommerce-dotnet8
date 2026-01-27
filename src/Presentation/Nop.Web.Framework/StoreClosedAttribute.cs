using System;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc.Filters;
// using System.Web.Mvc; - Removed for .NET 8
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Services.Topics;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Store closed attribute
    /// </summary>
    public class StoreClosedAttribute : ActionFilterAttribute
    {
        private readonly bool _ignore;

        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="ignore">Pass false in order to ignore this functionality for a certain action method</param>
        public StoreClosedAttribute(bool ignore = false)
        {
            this._ignore = ignore;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            //search the solution by "[StoreClosed(true)]" keyword 
            //in order to find method available even when a store is closed
            if (_ignore)
                return;


            Microsoft.AspNetCore.Http.HttpRequest request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            string actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor).ActionName;
            if (string.IsNullOrEmpty(actionName))
                return;

            string controllerName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)filterContext.ActionDescriptor).ControllerName;
            if (string.IsNullOrEmpty(controllerName))
                return;

            //don't apply filter to child methods
            // if (IsChildAction) - removed
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();
            if (!storeInformationSettings.StoreClosed)
                return;

            //topics accessible when a store is closed
            if (controllerName.Equals("Nop.Web.Controllers.TopicController", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("TopicDetails", StringComparison.InvariantCultureIgnoreCase))
            {
                var topicService = EngineContext.Current.Resolve<ITopicService>();
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                var allowedTopicIds = topicService.GetAllTopics(storeContext.CurrentStore.Id)
                    .Where(t => t.AccessibleWhenStoreClosed)
                    .Select(t => t.Id)
                    .ToList();
                var requestedTopicId = filterContext.RouteData.Values["topicId"] as int?;
                if (requestedTopicId.HasValue && allowedTopicIds.Contains(requestedTopicId.Value))
                    return;
            }

            //access to a closed store?
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            if (permissionService.Authorize(StandardPermissionProvider.AccessClosedStore))
                return;

            var storeClosedUrl = "/ StoreClosed" /* IUrlHelper creation simplified */;
            filterContext.Result = new Microsoft.AspNetCore.Mvc.RedirectResult(storeClosedUrl);
        }
    }
}
