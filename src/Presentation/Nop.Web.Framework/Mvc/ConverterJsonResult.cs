using System;
using Microsoft.AspNetCore.Mvc;
// using System.Web.Mvc; - Removed for .NET 8
using Newtonsoft.Json;
using Nop.Core;

namespace Nop.Web.Framework.Mvc
{
    /// <summary>
    /// Represents custom JsonResult with using Json converters
    /// </summary>
    public class ConverterJsonResult : JsonResult
    {
        #region Fields


        private readonly JsonConverter[] _converters;

        #endregion

        #region Ctor

        public ConverterJsonResult(params JsonConverter[] converters) : base(null)
        {
            _converters = converters;
        }

        #endregion

        #region Methods



        // ExecuteResult removed - ASP.NET Core JsonResult uses different execution model
        // Converters should be configured in JsonOptions globally

        #endregion
    }
}

