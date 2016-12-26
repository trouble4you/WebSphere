using System.Web.Mvc;

namespace WebSphere.WebUI.Controllers
{
    public class DeveloperController : Controller
    {
        public ActionResult Index()
        { 
            return View("Developer");
        }

        public ActionResult ThreadsStates()
        { 
            return View("TagsThreads");
        }

        public ActionResult OpcStates()
        { 
            return View("OpcThreads");
        }

        public ActionResult AlarmCfgStates()
        { 
            return View("AlarmThreads");
        }

        public ActionResult AlarmStates()
        { 
            return View("Alarms");
        }
        public ActionResult ReportCheck()
        {
            return View("ReportCheck");
        }
    }
}