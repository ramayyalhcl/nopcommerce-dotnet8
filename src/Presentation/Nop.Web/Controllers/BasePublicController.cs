using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Nop.Core.Infrastructure;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Seo;

namespace Nop.Web.Controllers
{
    // [CheckAffiliate] - attribute deleted
    [StoreClosed]
    [PublicStoreAllowNavigation]
    // [LanguageSeoCode] - attribute deleted
    [NopHttpsRequirement(SslRequirement.NoMatter)]
    [WwwRequirement]
    public abstract partial class BasePublicController : BaseController
    {
        protected virtual ActionResult InvokeHttp404()
        {
            // Redirect to 404 page (old MVC IController.Execute pattern removed in ASP.NET Core)
            return RedirectToAction("PageNotFound", "Common");
        }

    }
}
