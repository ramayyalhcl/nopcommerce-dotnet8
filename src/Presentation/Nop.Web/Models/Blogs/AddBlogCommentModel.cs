using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Blogs
{
    public partial class AddBlogCommentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Blog.Comments.CommentText")]
        // [AllowHtml] - removed in .NET 8
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}