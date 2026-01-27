using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.News
{
    public partial class AddNewsCommentModel : BaseNopModel
    {
        [NopResourceDisplayName("News.Comments.CommentTitle")]
        // [AllowHtml] - removed in .NET 8
        public string CommentTitle { get; set; }

        [NopResourceDisplayName("News.Comments.CommentText")]
        // [AllowHtml] - removed in .NET 8
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}