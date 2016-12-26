using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSphere.WebUI.Helpers
{
    public static class MenuActiveLink
    {
        public static string IsSelected(this HtmlHelper html, string controllers = "", string actions = "", string param = "", string cssClass = "app-focus tabs-li-focus")
        {
            ViewContext viewContext = html.ViewContext;
            bool isChildAction = viewContext.Controller.ControllerContext.IsChildAction;

            //string url = HttpContext.Current.Request.Url.AbsoluteUri;

            if (isChildAction)
            {
                viewContext = html.ViewContext.ParentActionViewContext;
            }

            RouteValueDictionary routeValues = viewContext.RouteData.Values;
            string currentAction = routeValues["action"].ToString();
            string currentController = routeValues["controller"].ToString();

            if (String.IsNullOrEmpty(actions))
            {
                actions = currentAction;
            }

            if (String.IsNullOrEmpty(controllers))
            {
                controllers = currentController;
            }

            string[] acceptedActions = actions.Trim().Split(',').Distinct().ToArray();
            string[] acceptedControllers = controllers.Trim().Split(',').Distinct().ToArray();

            #region param
            string queryParam = HttpContext.Current.Request.QueryString["mnemo"];
            string currentParameter = "";
            if (queryParam != null)
            {
                currentParameter = queryParam.ToString();
            }
            if (String.IsNullOrEmpty(param))
            {
                param = currentParameter;
            }
            string[] acceptedParams = param.Trim().Split(',').Distinct().ToArray();
            #endregion

            return acceptedActions.Contains(currentAction) && acceptedControllers.Contains(currentController) && acceptedParams.Contains(currentParameter) ?
                cssClass : String.Empty;
        }
    }
}