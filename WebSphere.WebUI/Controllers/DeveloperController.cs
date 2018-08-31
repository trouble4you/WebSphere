using System.Web.Mvc;

namespace WebSphere.WebUI.Controllers
{
    public class DeveloperController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string users = "")
        {
            return PartialView("OpcThreads");
        }


        public ActionResult ThreadsStates()
        {
            return View("TagsThreads");
        }
        public ActionResult Summ()
        {
            return View();
        }
        [HttpGet]
        public ActionResult OpcThreads()
        {
            return PartialView();
        }
        [HttpGet]
        public ActionResult AlarmCfgStates()
        {
            return PartialView("AlarmThreads");
        }
        [HttpGet]
        public ActionResult AlarmStates()
        {
            return PartialView("Alarms");
        }
        [HttpGet]
        public ActionResult ReportCheck()
        {
            return PartialView();
        }
    }
}