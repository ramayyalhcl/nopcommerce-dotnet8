using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
// using FluentValidation.Attributes; - removed (use DI registration)
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Install;

namespace Nop.Web.Models.Install
{
    // [Validator(typeof(InstallValidator))]
    public partial class InstallModel : BaseNopModel
    {
        public InstallModel()
        {
            this.AvailableLanguages = new List<SelectListItem>();
        }
        // [AllowHtml] - removed in .NET 8
        public string AdminEmail { get; set; }
        // [AllowHtml] - removed in .NET 8
        [NoTrim]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
        // [AllowHtml] - removed in .NET 8
        [NoTrim]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


        // [AllowHtml] - removed in .NET 8
        public string DatabaseConnectionString { get; set; }
        public string DataProvider { get; set; }
        public bool DisableSqlCompact { get; set; }
        //SQL Server properties
        public string SqlConnectionInfo { get; set; }
        // [AllowHtml] - removed in .NET 8
        public string SqlServerName { get; set; }
        // [AllowHtml] - removed in .NET 8
        public string SqlDatabaseName { get; set; }
        // [AllowHtml] - removed in .NET 8
        public string SqlServerUsername { get; set; }
        // [AllowHtml] - removed in .NET 8
        public string SqlServerPassword { get; set; }
        public string SqlAuthenticationType { get; set; }
        public bool SqlServerCreateDatabase { get; set; }

        public bool UseCustomCollation { get; set; }
        // [AllowHtml] - removed in .NET 8
        public string Collation { get; set; }


        public bool DisableSampleDataOption { get; set; }
        public bool InstallSampleData { get; set; }

        public List<SelectListItem> AvailableLanguages { get; set; }
    }
}