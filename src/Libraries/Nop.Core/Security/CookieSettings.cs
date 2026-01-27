using Microsoft.AspNetCore.Http;

namespace Nop.Core.Security
{
    public partial class CookieSettings
    {
        public CookieSecurePolicy SecurePolicy { get; set; } = CookieSecurePolicy.SameAsRequest;
        public SameSiteMode SameSiteMode { get; set; } = SameSiteMode.Lax;
    }
}
