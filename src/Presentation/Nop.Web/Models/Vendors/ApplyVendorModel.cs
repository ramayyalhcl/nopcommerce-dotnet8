using Microsoft.AspNetCore.Mvc;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Vendors;

namespace Nop.Web.Models.Vendors
{
    // [Validator(typeof(ApplyVendorValidator))]
    public partial class ApplyVendorModel : BaseNopModel
    {
        [NopResourceDisplayName("Vendors.ApplyAccount.Name")]
        // [AllowHtml] - removed in .NET 8
        public string Name { get; set; }

        [NopResourceDisplayName("Vendors.ApplyAccount.Email")]
        // [AllowHtml] - removed in .NET 8
        public string Email { get; set; }

        [NopResourceDisplayName("Vendors.ApplyAccount.Description")]
        // [AllowHtml] - removed in .NET 8
        public string Description { get; set; }
        
        public bool DisplayCaptcha { get; set; }

        public bool DisableFormInput { get; set; }
        public string Result { get; set; }
    }
}