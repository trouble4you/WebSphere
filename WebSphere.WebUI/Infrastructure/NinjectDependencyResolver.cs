using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using WebSphere.Alarms;
using WebSphere.ClientOPC;
using WebSphere.Domain.Entities;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Reports;

namespace WebSphere.WebUI.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel kernel;

        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            #region Ермолаев Е.В.

            // Аккаунт
            kernel.Bind<IAccount>().To<Account>();

            // JSON
            kernel.Bind<IJSON>().To<JSON>();

            // Логирование
            kernel.Bind<ILogging>().To<Logging>();

            // ContentType, Controllers, Actions, Permissions
            kernel.Bind<ICSContentType>().To<CSContentType>();

            #endregion


            #region Сагирова А.Н.
            //операции с деревом
            kernel.Bind<IJSTree>().To<JSTree>();
            //операции с тегами
            kernel.Bind<ITagConfigurator>().To<TagConfigurator>();
            //работа с файлами
            kernel.Bind<IFileWork>().To<FileWork>();
            #endregion


            #region Марков С.Л.

            //  
             kernel.Bind<ITrends>().To<Trends.Trend>();
            // OPC
             kernel.Bind<IOpcPoller>().To<OpcPoller>();
 
            // Alarms

               kernel.Bind<IAlarmServer>().To<AlarmServer>();
            // Reports
             kernel.Bind<IReportServer>().To<ReportServer>();
            #endregion
        }
    }
}