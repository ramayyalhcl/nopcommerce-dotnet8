using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    // [Validator(typeof(AddressValidator))]
    public partial class AddressModel : BaseNopEntityModel
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            CustomAddressAttributes = new List<AddressAttributeModel>();
        }

        [NopResourceDisplayName("Address.Fields.FirstName")]
        // [AllowHtml] - removed in .NET 8
        public string FirstName { get; set; }
        [NopResourceDisplayName("Address.Fields.LastName")]
        // [AllowHtml] - removed in .NET 8
        public string LastName { get; set; }
        [NopResourceDisplayName("Address.Fields.Email")]
        // [AllowHtml] - removed in .NET 8
        public string Email { get; set; }


        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.Company")]
        // [AllowHtml] - removed in .NET 8
        public string Company { get; set; }

        public bool CountryEnabled { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        public int? CountryId { get; set; }
        [NopResourceDisplayName("Address.Fields.Country")]
        // [AllowHtml] - removed in .NET 8
        public string CountryName { get; set; }

        public bool StateProvinceEnabled { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        [NopResourceDisplayName("Address.Fields.StateProvince")]
        // [AllowHtml] - removed in .NET 8
        public string StateProvinceName { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.City")]
        // [AllowHtml] - removed in .NET 8
        public string City { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.Address1")]
        // [AllowHtml] - removed in .NET 8
        public string Address1 { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        [NopResourceDisplayName("Address.Fields.Address2")]
        // [AllowHtml] - removed in .NET 8
        public string Address2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.ZipPostalCode")]
        // [AllowHtml] - removed in .NET 8
        public string ZipPostalCode { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.PhoneNumber")]
        // [AllowHtml] - removed in .NET 8
        public string PhoneNumber { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
        [NopResourceDisplayName("Address.Fields.FaxNumber")]
        // [AllowHtml] - removed in .NET 8
        public string FaxNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }


        public string FormattedCustomAddressAttributes { get; set; }
        public IList<AddressAttributeModel> CustomAddressAttributes { get; set; }
    }
}