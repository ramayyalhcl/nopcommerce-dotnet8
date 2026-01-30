using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using System.Web.Mvc.Html; - removed for .NET 8
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Framework.Localization;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Common;
using Nop.Web.Factories;

namespace Nop.Web.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// BBCode editor
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HTML Helper</param>
        /// <param name="name">Name</param>
        /// <returns>Editor</returns>
        public static IHtmlContent BBCodeEditor<TModel>(this IHtmlHelper<TModel> html, string name)
        {
            var sb = new StringBuilder();

            var storeLocation = EngineContext.Current.Resolve<IWebHelper>().GetStoreLocation();
            string bbEditorWebRoot = String.Format("{0}Content/", storeLocation);

            sb.AppendFormat("<script src=\"{0}Content/BBEditor/ed.js\" type=\"{1}\"></script>", storeLocation, MimeTypes.TextJavascript);
            sb.AppendLine();
            sb.AppendFormat("<script language=\"javascript\" type=\"{0}\">", MimeTypes.TextJavascript);
            sb.AppendLine();
            sb.AppendFormat("edToolbar('{0}','{1}');", name, bbEditorWebRoot);
            sb.AppendLine();
            sb.Append("</script>");
            sb.AppendLine();

            return new HtmlString(sb.ToString());
        }

        //we have two pagers:
        //The first one can have custom routes
        //The second one just adds query string parameter
        public static IHtmlContent Pager<TModel>(this IHtmlHelper<TModel> html, PagerModel model)
        {
            if (model.TotalRecords == 0)
                return null;

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(model.CurrentPageText, model.PageIndex + 1, model.TotalPages, model.TotalRecords));
                links.Append("</li>");
            }
            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.page = 1;

                        links.Append("<li class=\"first-page\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.FirstPageTitle") }));
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.FirstPageTitle") }));
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.page = (model.PageIndex);

                        links.Append("<li class=\"previous-page\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.PreviousPageTitle") }));
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.PreviousPageTitle") }));
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowIndividualPages)
                {
                    //individual pages
                    int firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    int lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (int i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page\"><span>{0}</span></li>", (i + 1));
                        }
                        else
                        {
                            model.RouteValues.page = (i + 1);

                            links.Append("<li class=\"individual-page\">");
                            if (model.UseRouteLinks)
                            {
                                links.Append(html.RouteLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), (i + 1)) }));
                            }
                            else
                            {
                                links.Append(html.ActionLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), (i + 1)) }));
                            }
                            links.Append("</li>");
                        }
                    }
                }
                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.page = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.NextPageTitle") }));
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.NextPageTitle") }));
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.page = model.TotalPages;

                        links.Append("<li class=\"last-page\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.LastPageTitle") }));
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.LastPageTitle") }));
                        }
                        links.Append("</li>");
                    }
                }
            }
            var result = links.ToString();
            if (!String.IsNullOrEmpty(result))
            {
                result = "<ul>" + result + "</ul>";
            }
            return new HtmlString(result);
        }
        public static IHtmlContent ForumTopicSmallPager<TModel>(this IHtmlHelper<TModel> html, ForumTopicRowModel model)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            var forumTopicId = model.Id;
            var forumTopicSlug = model.SeName;
            var totalPages = model.TotalPostPages;

            if (totalPages > 0)
            {
                var links = new StringBuilder();

                if (totalPages <= 4)
                {
                    for (int x = 1; x <= totalPages; x++)
                    {
                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, page = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }));
                        if (x < totalPages)
                        {
                            links.Append(", ");
                        }
                    }
                }
                else
                {
                    links.Append(html.RouteLink("1", "TopicSlugPaged", new { id = forumTopicId, page = (1), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), 1) }));
                    links.Append(" ... ");

                    for (int x = (totalPages - 2); x <= totalPages; x++)
                    {
                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, page = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }));

                        if (x < totalPages)
                        {
                            links.Append(", ");
                        }
                    }
                }

                // Inserts the topic page links into the localized string ([Go to page: {0}])
                return new HtmlString(String.Format(localizationService.GetResource("Forum.Topics.GotoPostPager"), links.ToString()));
            }
            return HtmlString.Empty;
        }
        
        // Pager method removed - Pager class deleted (needs reimplementation)



        /// <summary>
        /// Get topic system name
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="systemName">System name</param>
        /// <returns>Topic SEO Name</returns>
        public static string GetTopicSeName<T>(this IHtmlHelper<T> html, string systemName)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();

            //static cache manager
            var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_SENAME_BY_SYSTEMNAME, systemName, workContext.WorkingLanguage.Id, storeContext.CurrentStore.Id);
            var cachedSeName = cacheManager.Get(cacheKey, () =>
            {
                var topicService = EngineContext.Current.Resolve<ITopicService>();
                var topic = topicService.GetTopicBySystemName(systemName, storeContext.CurrentStore.Id);
                if (topic == null)
                    return "";

                return topic.GetSeName();
            });
            return cachedSeName;
        }

        /// <summary>
        /// Render widgets for a widget zone
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="widgetZone">Widget zone name</param>
        /// <returns>Widget content</returns>
        public static IHtmlContent Widget<TModel>(this IHtmlHelper<TModel> html, string widgetZone)
        {
            return Widget(html, widgetZone, null);
        }

        /// <summary>
        /// Render widgets for a widget zone with additional data
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Widget content</returns>
        public static IHtmlContent Widget<TModel>(this IHtmlHelper<TModel> html, string widgetZone, object additionalData)
        {
            if (string.IsNullOrEmpty(widgetZone))
                return HtmlString.Empty;

            try
            {
                var widgetModelFactory = EngineContext.Current.Resolve<IWidgetModelFactory>();
                var model = widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

                if (model == null || !model.Any())
                    return HtmlString.Empty;

                // Render the WidgetsByZone partial view
                return html.Partial("WidgetsByZone", model);
            }
            catch (Exception)
            {
                // Return empty on error
                return HtmlString.Empty;
            }
        }

        /// <summary>
        /// Render a controller action as a partial view (replaces Html.Action from ASP.NET MVC)
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <returns>Action content</returns>
        public static IHtmlContent Action<TModel>(this IHtmlHelper<TModel> html, string actionName, string controllerName)
        {
            return Action(html, actionName, controllerName, null);
        }

        /// <summary>
        /// Render a controller action as a partial view with route values
        /// Note: This implementation invokes the controller action directly.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <returns>Action content</returns>
        public static IHtmlContent Action<TModel>(this IHtmlHelper<TModel> html, string actionName, string controllerName, object routeValues)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return HtmlString.Empty;

            try
            {
                var httpContext = html.ViewContext.HttpContext;
                var serviceProvider = httpContext.RequestServices;
                
                // Get controller instance from DI
                var controllerType = Type.GetType($"Nop.Web.Controllers.{controllerName}Controller, Nop.Web");
                if (controllerType == null)
                {
                    // Try without Nop.Web prefix for external controllers
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        controllerType = assembly.GetType($"Nop.Web.Controllers.{controllerName}Controller");
                        if (controllerType != null) break;
                    }
                }
                
                if (controllerType == null)
                    return HtmlString.Empty;

                var controller = Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(serviceProvider, controllerType) as Microsoft.AspNetCore.Mvc.Controller;
                
                if (controller == null)
                    return HtmlString.Empty;

                // Set controller context
                controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
                {
                    HttpContext = httpContext
                };

                // Invoke action method
                var method = controllerType.GetMethod(actionName);
                if (method == null)
                    return HtmlString.Empty;

                // Prepare parameters
                var parameters = new List<object>();
                if (routeValues != null)
                {
                    var routeDict = new Microsoft.AspNetCore.Routing.RouteValueDictionary(routeValues);
                    var methodParams = method.GetParameters();
                    foreach (var param in methodParams)
                    {
                        if (routeDict.ContainsKey(param.Name))
                            parameters.Add(routeDict[param.Name]);
                        else if (param.HasDefaultValue)
                            parameters.Add(param.DefaultValue);
                        else
                            parameters.Add(null);
                    }
                }

                var result = method.Invoke(controller, parameters.Any() ? parameters.ToArray() : null);
                
                // Handle result
                if (result is Microsoft.AspNetCore.Mvc.ContentResult contentResult)
                    return new HtmlString(contentResult.Content);
                else if (result is Microsoft.AspNetCore.Mvc.PartialViewResult partialViewResult)
                    return html.Partial(partialViewResult.ViewName ?? actionName, partialViewResult.Model);
                else if (result is Microsoft.AspNetCore.Mvc.ViewResult viewResult)
                    return html.Partial(viewResult.ViewName ?? actionName, viewResult.Model);
                
                return HtmlString.Empty;
            }
            catch (Exception ex)
            {
                // .NET 8.0: Added detailed logging to diagnose Html.Action failures
                var logger = EngineContext.Current.Resolve<Microsoft.Extensions.Logging.ILogger<object>>();
                logger.LogError(ex, "Html.Action failed for {ActionName}/{ControllerName}: {Message}", 
                    actionName, controllerName, ex.Message);
                
                // Return empty on error - this allows the page to continue loading
                return HtmlString.Empty;
            }
        }

        /// <summary>
        /// Render widgets for a widget zone (non-generic overload for dynamic)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="widgetZone">Widget zone name</param>
        /// <returns>Widget content</returns>
        public static IHtmlContent Widget(this IHtmlHelper<dynamic> html, string widgetZone)
        {
            return Widget(html, widgetZone, null);
        }

        /// <summary>
        /// Render widgets for a widget zone with additional data (non-generic overload for dynamic)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Widget content</returns>
        public static IHtmlContent Widget(this IHtmlHelper<dynamic> html, string widgetZone, object additionalData)
        {
            if (string.IsNullOrEmpty(widgetZone))
                return HtmlString.Empty;

            try
            {
                var widgetModelFactory = EngineContext.Current.Resolve<IWidgetModelFactory>();
                var model = widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

                if (model == null || !model.Any())
                    return HtmlString.Empty;

                // Render the WidgetsByZone partial view
                return html.Partial("WidgetsByZone", model);
            }
            catch (Exception)
            {
                // Return empty on error
                return HtmlString.Empty;
            }
        }

        /// <summary>
        /// Render a controller action as a partial view (non-generic overload for dynamic)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <returns>Action content</returns>
        public static IHtmlContent Action(this IHtmlHelper<dynamic> html, string actionName, string controllerName)
        {
            return Action(html, actionName, controllerName, null);
        }

        /// <summary>
        /// Render a controller action as a partial view with route values (non-generic overload for dynamic)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <returns>Action content</returns>
        public static IHtmlContent Action(this IHtmlHelper<dynamic> html, string actionName, string controllerName, object routeValues)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return HtmlString.Empty;

            try
            {
                // Extract model from route values if present
                object model = null;
                if (routeValues != null)
                {
                    var routeDict = new Microsoft.AspNetCore.Routing.RouteValueDictionary(routeValues);
                    // Try to get a model property from route values
                    if (routeDict.ContainsKey("model"))
                        model = routeDict["model"];
                    else if (routeDict.Count == 1)
                        model = routeDict.Values.First();
                }

                // Try to render the partial view directly
                // The view should be at Views/{ControllerName}/{ActionName}.cshtml
                var viewName = $"{controllerName}/{actionName}";
                
                // First try the specific view name
                var result = html.Partial(viewName, model);
                if (result != null)
                    return result;

                // If that fails, try just the action name
                return html.Partial(actionName, model) ?? HtmlString.Empty;
            }
            catch (Exception)
            {
                // Return empty on error - this allows the page to continue loading
                return HtmlString.Empty;
            }
        }

        /// <summary>
        /// Get localized string (T helper for Razor views)
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="arguments">Arguments for string formatting</param>
        /// <returns>Localized string</returns>
        public static LocalizedString T<TModel>(this IHtmlHelper<TModel> html, string resourceName, string defaultValue = "", params object[] arguments)
        {
            try
            {
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                
                var resourceValue = localizationService.GetResource(resourceName, workContext.WorkingLanguage.Id, true, defaultValue);
                
                if (arguments != null && arguments.Length > 0)
                {
                    resourceValue = string.Format(resourceValue, arguments);
                }
                
                return new LocalizedString(resourceValue);
            }
            catch
            {
                return new LocalizedString(defaultValue ?? resourceName);
            }
        }

        /// <summary>
        /// Get localized string (T helper for Razor views - non-generic overload)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="arguments">Arguments for string formatting</param>
        /// <returns>Localized string</returns>
        public static LocalizedString T(this IHtmlHelper<dynamic> html, string resourceName, string defaultValue = "", params object[] arguments)
        {
            try
            {
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                
                var resourceValue = localizationService.GetResource(resourceName, workContext.WorkingLanguage.Id, true, defaultValue);
                
                if (arguments != null && arguments.Length > 0)
                {
                    resourceValue = string.Format(resourceValue, arguments);
                }
                
                return new LocalizedString(resourceValue);
            }
            catch
            {
                return new LocalizedString(defaultValue ?? resourceName);
            }
        }

        /// <summary>
        /// JavaScript string encode (replaces HttpUtility.JavaScriptStringEncode)
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns>Encoded string</returns>
        public static string JavaScriptStringEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            
            return JavaScriptEncoder.Default.Encode(value);
        }

        /// <summary>
        /// nopCommerce dropdown list
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HTML Helper</param>
        /// <param name="name">Name</param>
        /// <param name="selectList">Select list</param>
        /// <param name="htmlAttributes">HTML attributes</param>
        /// <returns>HTML</returns>
        public static IHtmlContent NopDropDownList<TModel>(this IHtmlHelper<TModel> html, string name, IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> selectList, object htmlAttributes = null)
        {
            return html.DropDownList(name, selectList, htmlAttributes);
        }

        /// <summary>
        /// nopCommerce editor for
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <typeparam name="TValue">Value</typeparam>
        /// <param name="html">HTML Helper</param>
        /// <param name="expression">Expression</param>
        /// <returns>HTML</returns>
        public static IHtmlContent NopEditorFor<TModel, TValue>(this IHtmlHelper<TModel> html, System.Linq.Expressions.Expression<Func<TModel, TValue>> expression)
        {
            return html.EditorFor(expression);
        }
    }
}

