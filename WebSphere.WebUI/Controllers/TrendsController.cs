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
using WebSphere.WebUI.Models; 

namespace WebTelemetrySphere.Controllers
{
    public class TrendsController : Controller
    {
        private ITrends _Trends;
        private IJSON _json;

        public TrendsController(ITrends trends, IJSON json)
         {
             _Trends = trends;
             _json = json; 
         }
        private EFDbContext context = new EFDbContext();
        // GET: Reports
        public ActionResult Index()
        {
            return View("Trends");
        }
  
        public ActionResult Trends(int id)
        {
            var mtp = new WebSphere.WebUI.Models.MyTrendPage();
            var _object =
                        context.Objects.FirstOrDefault(x => (x.Id == id));

            if (_object != null)
            {
                var objectId = id;
                string objectName = _object.Name;
                var typeId = _object.Type;

                var _object_type =
                    context.ObjectTypes.FirstOrDefault(x => (x.Id == _object.Type));
                string objectTypeStr = "";
                if (_object_type != null)
                {
                    objectTypeStr = _object_type.Name;
                }
                else objectTypeStr = "Объект";

                var objects_signals = (from ti in context.Objects
                    join to in context.Properties on ti.Id equals to.ObjectId
                    where ti.Type == 2 && to.PropId == 0 && ti.ParentId == _object.Id
                                       select new { Id = ti.Id, Prop = to.Value });
                var signalslist = objects_signals.ToList();
                foreach (var signal in signalslist)
                {
                    dynamic dict = JsonConvert.DeserializeObject(signal.Prop);
                    string signalId = Convert.ToString(signal.Id);
                    string signalName = Convert.ToString(dict.Name);

                    var _signal = new OrderedDictionary();
                    _signal.Add("signal_name", signalName);
                    _signal.Add("signal_id", signalId);
                    mtp.signals.Add(_signal);
                }

                DateTime dt2 = DateTime.Now;
                DateTime dt1 = dt2.AddHours(-1);



                mtp.start_date = (ulong) ConvertToUnixTimestamp(dt1);
                 mtp.end_date = (ulong) ConvertToUnixTimestamp(dt2);
                 // mtp.start_date= WebSphere.WebUI.MyTime.GetDatetimeFromDate(dt1);
               // mtp.end_date = WebSphere.WebUI.MyTime.GetDatetimeFromDate(dt2);

                mtp.object_type_str = objectTypeStr;

                mtp.object_name = objectName;

                mtp.object_id = objectId;
            }
            return View("Trends", mtp);
        }
        public ActionResult GetData(int id, string signs, string sd, string ed)
        {
            var TrendData = _Trends.GetTrend(sd, ed, signs);
            return Json(TrendData);
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