using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;
using WebSphere.WebUI.Models;


namespace WebSphere.WebUI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        IJSON _json;
        // Аккаунт
        IAccount account;
        IReportServer _reports;

        public ReportsController(IReportServer reports, IJSON json, IAccount account)
        {
            this.account = account;
            this._reports = reports;
            this._json = json;
        }


        public ActionResult Reports()
        {
            return View(_reports.ObjectList());
        }


        public ActionResult GetReport(int id, string parametrs)
        {
            Report rezultList = null;

            var values = (Dictionary<string, dynamic>)_json.Deserialize(parametrs, typeof(Dictionary<string, dynamic>));

            try
            {
                rezultList = _reports.GetReport(id, values);
            }
            catch (Exception ex)
            {
                return Json(rezultList);
            }
            return Json(rezultList);
        }

        public ActionResult GetListJournal()
        {
            List<Report> rezultList = null;

            try
            {
                rezultList = _reports.ReportList();
            }
            catch (Exception ex)
            {
                return Json(rezultList);
            }
            return Json(rezultList);
        }
        public ActionResult GetListObjectsChlds(int parentId)
        {
            List<WebSphere.Domain.Abstract.AGZUObject> rezultList = null;

            try
            {
                rezultList = _reports.ChildList(parentId);
            }
            catch (Exception ex)
            {
                return Json(rezultList);
            }
            return Json(rezultList);
        }
        public ActionResult GetObjectList()
        {
            List<Report> rezultList = null;

            try
            {
                rezultList = _reports.ReportList();
            }
            catch (Exception ex)
            {
                return Json(rezultList);
            }
            return Json(rezultList);
        }

        public FileContentResult GetExcel(int journals, string datetimepicker_start, string datetimepicker_end, string op1, string op2)
        {
            MemoryStream rezultList = null;
            string journal;
            var values = new Dictionary<string, dynamic>
            {
                {"StartDate", datetimepicker_start},
                {"EndDate", datetimepicker_end},
                {"Operator1", op1},
                {"Operator2", op2}
            };

            try
            {
                rezultList = _reports.GetExcel(Convert.ToInt32(journals), values, out journal);
            }

            catch (Exception ex)
            {
                return null;
            }
            if (rezultList != null)
            {
                var dateTime = DateTime.Now;
                return File(rezultList.ToArray(), "application/vnd.ms-excel", "" + "" + journal + "  от " + dateTime + ".xls");
            }
            else
                return null;
        }
    }
}