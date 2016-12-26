using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSphere.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Mnemo", "Mnemo", new { mnemo = "Meteli" });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult UpdatePoints()
        {
            // текущее время
            string currentTime = DateTime.Now.ToString("dd.MM.yy HH:mm:ss");
            return Json(new { result = "time", time = currentTime });
        }
	}
}