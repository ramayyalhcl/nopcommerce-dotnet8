using Microsoft.AspNetCore.Mvc;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    // [Validator(typeof(ContactUsValidator))]
    public partial class ContactUsModel : BaseNopModel
    {
        // [AllowHtml] - removed in .NET 8
        [NopResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        // [AllowHtml] - removed in .NET 8
        [NopResourceDisplayName("ContactUs.Subject")]
        public string Subject { get; set; }
        public bool SubjectEnabled { get; set; }

        // [AllowHtml] - removed in .NET 8
        [NopResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        // [AllowHtml] - removed in .NET 8
        [NopResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}