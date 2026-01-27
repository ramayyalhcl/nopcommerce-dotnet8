using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
// System.Web removed
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Security.Captcha
{
    public static class HtmlExtensions
    {
        public static string GenerateCaptcha(this IHtmlHelper helper)
        {
            // Simplified for .NET 8 - GRecaptchaControl removed (HtmlTextWriter dependency)
            // TODO: Implement with direct HTML generation or tag helper
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();
            return $"<!-- Captcha placeholder - key: {captchaSettings.ReCaptchaPublicKey} -->";
        }
    }
}

