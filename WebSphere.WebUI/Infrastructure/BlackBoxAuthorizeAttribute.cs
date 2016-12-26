using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebSphere.Domain.Concrete;
using WebSphere.WebUI.App_Start;

namespace WebSphere.WebUI.Infrastructure
{
    public class BlackBoxAuthorizeAttribute : AuthorizeAttribute
    {
        // аккаунт
        private static readonly Account account = new Account();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorized = base.AuthorizeCore(httpContext);

            // если не аутентифицирован
            if (!authorized)
            {
                return false;
            }

            // логин
            var username = httpContext.User.Identity.Name;

            // данные маршрута
            var routeData = httpContext.Request.RequestContext.RouteData;

            // контроллер
            var controller = routeData.GetRequiredString("controller");

            // метод
            var action = routeData.GetRequiredString("action");

            // список всех ролей (загружен при старте приложения)
            var roles = RoleConfig.GetAllRoles();

            // проверяем права доступа
            return account.HasAccess(controller, action, username, roles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // ошибка 403 - отказ в доступе
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Error" }, 
                        { "action", "Error_403" }
                    }
                );
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }

    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    //public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    //{
    //    protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
    //    {
    //        if (filterContext.HttpContext.Request.IsAuthenticated)
    //        {
    //            filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
    //        }
    //        else
    //        {
    //            base.HandleUnauthorizedRequest(filterContext);
    //        }
    //    }
    //}
}