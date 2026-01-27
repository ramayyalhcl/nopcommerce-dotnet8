using System;
using Microsoft.AspNetCore.Mvc.Filters;
// using System.Web.Mvc; - Removed for .NET 8

namespace Nop.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)] 
    public class ParameterBasedOnFormNameAndValueAttribute : Attribute, IActionFilter
    {
        private readonly string _name;
        private readonly string _value;
        private readonly string _actionParameterName;

        public ParameterBasedOnFormNameAndValueAttribute(string name, string value, string actionParameterName)
        {
            this._name = name;
            this._value = value;
            this._actionParameterName = actionParameterName;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Form.TryGetValue(_name, out var formValue))
            {
                filterContext.ActionArguments[_actionParameterName] = !string.IsNullOrEmpty(formValue) &&
                                                                       formValue.ToString().ToLowerInvariant().Equals(_value.ToLowerInvariant());
            }
            else
            {
                filterContext.ActionArguments[_actionParameterName] = false;
            }
        }
    }
}

