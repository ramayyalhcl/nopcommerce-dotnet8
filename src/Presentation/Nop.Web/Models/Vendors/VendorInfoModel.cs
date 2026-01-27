using Microsoft.AspNetCore.Mvc;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Vendors;

namespace Nop.Web.Models.Vendors
{
    // [Validator(typeof(VendorInfoValidator))]
    public class VendorInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Account.VendorInfo.Name")]
        // [AllowHtml] - removed in .NET 8
        public string Name { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Email")]
        // [AllowHtml] - removed in .NET 8
        public string Email { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Description")]
        // [AllowHtml] - removed in .NET 8
        public string Description { get; set; }

        [NopResourceDisplayName("Account.VendorInfo.Picture")]
        public string PictureUrl { get; set; }
    }
}