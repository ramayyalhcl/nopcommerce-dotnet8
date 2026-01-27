using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Nop.Web.Factories;
using Nop.Web.Framework;

namespace Nop.Web.Controllers
{
    public partial class CountryController : BasePublicController
	{
		#region Fields

        private readonly ICountryModelFactory _countryModelFactory;

	    #endregion

		#region Constructors

        public CountryController(ICountryModelFactory countryModelFactory)
		{
            this._countryModelFactory = countryModelFactory;
		}

        #endregion


        #region States / provinces

        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        [HttpGet]
        public virtual ActionResult GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            var model = _countryModelFactory.GetStatesByCountryId(countryId, addSelectStateItem);
            return Json(model/* JsonRequestBehavior removed in ASP.NET Core */);
        }

        #endregion
    }
}
