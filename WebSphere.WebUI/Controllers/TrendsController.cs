using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere;
using WebSphere.WebUI;
using WebSphere.WebUI.Models;

namespace WebTelemetrySphere.Controllers
{
    public class TrendsController : Controller
    {
        private ITrends _Trends;
        private IJSON _json;
        public class TSqlPropsTag
        {
            public int Id;
            public string Name = "";
            public string Alias = "";
            public int Opc;
            public string Connection = "";
            public string Description = "";
            public int ControllerType;
            public int RealType;
            public string Register = "";
            public int AccessType;
            public int Order;
            public double InMin;
            public double InMax;
            public double OutMin;
            public double OutMax;
            public string IsSpecialTag = "";
            public bool History_IsPermit;
            public int RegPeriod;
            public double Deadbend;
            public int State;
            public bool UpdateAnyway = false;
            //public Event Events;
            //public Alarm Alarms;
        }
        public TrendsController(ITrends trends, IJSON json)
        {
            _Trends = trends;
            _json = json;
        }
        private EFDbContext context = new EFDbContext();

        public ActionResult Index()
        {
            return View("Trends");
        }

        public ActionResult Trends(int id)
        {
            var mtp = _Trends.GetTrend(id);

            return View("Trends", mtp);
        }
        public ActionResult GetData(string signal_id, string start_date, string end_date)
        {
            return Json(MvcApplication.Trends.GetTrend(start_date, end_date, signal_id));
        }

        public ActionResult GetSignals(int id)
        {
             
            var signals = new List<Signal>();

            var mtp = _Trends.GetTrend(id);
           
            var objects_signals = (from ti in context.Objects
                                   join to in context.Properties on ti.Id equals to.ObjectId
                                   where ti.Type == 2 && to.PropId == 0 && ti.ParentId == id
                                   select new { Id = ti.Id, Prop = to.Value }); 
 

            return Json(mtp.signals, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetMaskSignals(int id, string signs)
        {
            var _object =
                        context.Objects.FirstOrDefault(x => (x.Id == id));
            List<OrderedDictionary> signals = new List<OrderedDictionary>();
            var objects_signals = (from ti in context.Objects
                                   join to in context.Properties on ti.Id equals to.ObjectId
                                   where ti.Type == 2 && to.PropId == 0 && ti.ParentId == _object.Id
                                   select new { Id = ti.Id, Prop = to.Value }).ToList();

            var signalslist = objects_signals.Where(x => x.Id > 0);
            if (signs != "0")
                signalslist = objects_signals.Where(x => x.Prop.Contains(signs));
            foreach (var signal in signalslist)
            {
                dynamic dict = JsonConvert.DeserializeObject(signal.Prop);
                string signalId = Convert.ToString(signal.Id);
                string signalName = Convert.ToString(dict.Name);

                var _signal = new OrderedDictionary();
                _signal.Add("signal_name", signalName);
                _signal.Add("signal_id", signalId);
                signals.Add(_signal);
            }

            return Json(signals);
        }
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            TimeSpan diff = date.ToLocalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            return origin.AddSeconds(timestamp);
        }
    }
}