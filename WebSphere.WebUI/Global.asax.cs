using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using WebSphere.Alarms;
using WebSphere.ClientOPC;
using WebSphere.Trends;
using WebSphere.Domain.Abstract;
using WebSphere.WebUI.App_Start;

namespace WebSphere.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {

        public static IOpcPoller OpcPoller { get; set; }
        public static IAlarmServer AlarmServer { get; private set; }
        public static ITrends Trends { get;   set; }
  
        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();

            // Тестировщик запросов к БД
            HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

            GlobalConfiguration.Configure(WebApiConfig.Register);

            // регистрация глобальных фильтров
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // регистратор маршрутов
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // хранилище ролей
            RoleConfig.RepositoryRoles();

            OpcPoller = new OpcPoller();
            AlarmServer = new AlarmServer();
            Trends = new Trend(); 
          OpcPoller.Init();
          AlarmServer.Init();
          AlarmServer.Run(); 
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            
        }

        protected void Application_End(object sender, EventArgs e)
        {
            
        }
    }
}
