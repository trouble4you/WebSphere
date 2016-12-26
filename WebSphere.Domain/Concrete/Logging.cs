using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;

namespace WebSphere.Domain.Concrete
{
    public class Logging : ILogging
    {
        private static log4net.ILog log { get; set; }

        public bool Logged(string logEventLevel, string message, string nameSpace, string currClass)
        {
            // имя текущего метода берем из фрейма 1
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            var currMethod = sf.GetMethod().Name;

            // строка лога
            var logString = nameSpace + " -> " + currClass + " -> " + currMethod;

            // лог
            log = log4net.LogManager.GetLogger(logString);

            // уровень логирования
            switch (logEventLevel)
            {
                case "Fatal":
                    log.Fatal(message);
                    break;

                case "Error":
                    log.Error(message);
                    break;

                case "Warn":
                    log.Warn(message);
                    break;

                case "Info":
                    log.Info(message);
                    break;

                case "Debug":
                    log.Debug(message);
                    break;
            }

            return true;
        }
    }
}
