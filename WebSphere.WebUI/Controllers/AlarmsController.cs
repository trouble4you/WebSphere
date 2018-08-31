using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Net.Http.Formatting;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
//using Newtonsoft.Json;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.Models;
//using WebSphere.Alarms; 


namespace WebSphere.WebUI.Controllers
{
    [Authorize]
    public class AlarmsController : Controller
    {


        public ActionResult Alarms(int? id = null)
        {
            if (id == null) ViewBag.Id = 0;
            else ViewBag.Id = id;
            return View();
        }
        public ActionResult Events(int? id = null)
        {
            if (id == null) ViewBag.Id = 0;
            else ViewBag.Id = id;
            return View();
        }

        public ActionResult SetAlarmAck(int id)
        {
            var result = (MvcApplication.AlarmServer.SetAlarmAck(id));
            return Json(result);
        }

        public ActionResult SetAlarmAckAll()
        {
            var result = (MvcApplication.AlarmServer.SetAlarmAckAll());
            return Json(result);
        }

        public ActionResult GetCurrentAlarms(Int32? id)
        {
            if (id == 0) id = null;
            var alarms = MvcApplication.AlarmServer.GetCurrentAlarms(id);
            return Json(alarms);
        }

        public ActionResult GetCurrentEvents(Int32? id)
        {
            if (id == 0) id = null;
            var events = MvcApplication.AlarmServer.GetCurrentEvents(id);
            return Json(events);
        }
        public ActionResult SoundAlarm()
        {
            var events = MvcApplication.AlarmServer.SoundAlarm();
            var json = Json(events);
            return json;
        }

        public ActionResult GetAlarmsCfgStates()
        {
            //var result = TagThreadsManager.GetThreadStates();
            var result = MvcApplication.AlarmServer.GetAlarmConfig();
            return Json(result);
        }
        public ActionResult RestartAlarms()
        {
            var result = MvcApplication.AlarmServer.Restart();
            return Json(result);
        }
    }
}