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
             
         
        public ActionResult Alarms()
        {
            return View();
        }
 
        public ActionResult SetAlarmAck(int id)
        {
            
  MvcApplication.AlarmServer.SetAlarmAck(id);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult SetAlarmAckAll()
        {
            MvcApplication.AlarmServer.SetAlarmAckAll();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        } 
    }
}