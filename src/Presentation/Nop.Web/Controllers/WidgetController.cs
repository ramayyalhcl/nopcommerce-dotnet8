using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;

namespace Nop.Web.Controllers
{
    public partial class WidgetController : BasePublicController
    {
		#region Fields

        private readonly IWidgetModelFactory _widgetModelFactory;

        #endregion

        #region Ctor

        public WidgetController(IWidgetModelFactory widgetModelFactory)
        {
            this._widgetModelFactory = widgetModelFactory;
        }

        #endregion

        #region Methods

        // // [ChildActionOnly] - removed in .NET 8, use ViewComponent - removed in .NET 8
        public virtual ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {
            var model = _widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

            //no data?
            if (!model.Any())
                return Content("");

            return PartialView(model);
        }

        #endregion
    }
}
