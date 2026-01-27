using Microsoft.AspNetCore.Mvc;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.PrivateMessages;

namespace Nop.Web.Models.PrivateMessages
{
    // [Validator(typeof(SendPrivateMessageValidator))]
    public partial class SendPrivateMessageModel : BaseNopEntityModel
    {
        public int ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public int ReplyToMessageId { get; set; }

        // [AllowHtml] - removed in .NET 8
        public string Subject { get; set; }

        // [AllowHtml] - removed in .NET 8
        public string Message { get; set; }
    }
}