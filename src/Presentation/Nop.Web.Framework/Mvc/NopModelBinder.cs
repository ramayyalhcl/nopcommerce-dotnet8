using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nop.Web.Framework.Mvc
{
    /// <summary>
    /// Nop model binder - updated for ASP.NET Core
    /// </summary>
    public class NopModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new System.ArgumentNullException(nameof(bindingContext));

            // Simplified for ASP.NET Core - BaseNopModel BindModel called via model binding pipeline
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
