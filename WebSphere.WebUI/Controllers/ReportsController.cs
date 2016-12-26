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
          IReportServer _reports;  

        public ReportsController(IReportServer reports,IJSON json)
        {
            this._reports = reports; 
            this._json = json; 
        }
 




        public ActionResult Reports()
        {
            return View();
        }
         

        public ActionResult GetReport(int id, string parametrs)
        {
            Report rezultList=null;

            var values = (Dictionary<string, dynamic>) _json.Deserialize(parametrs, typeof(Dictionary<string, dynamic>));
             
            try
            {
                rezultList = _reports.GetReport(id, values);
            }
            catch (Exception ex)
            {
                return Json (rezultList) ;
            }
            return Json(rezultList);
        }     
        
        public ActionResult GetListJournal()
        {
            List<Report> rezultList=null;
             
            try
            {
                rezultList = _reports.ReportList();
            }
            catch (Exception ex)
            {
                return Json (rezultList) ;
            }
            return Json(rezultList);
        }
         
        public FileContentResult GetExcel(int journals,  string datetimepicker_start,string datetimepicker_end)
        {
            MemoryStream rezultList = null;
            string journal;
            var values = new Dictionary<string, dynamic>
            {
                {"StartDate", datetimepicker_start},
                {"EndDate", datetimepicker_end}
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
                return File(rezultList.ToArray(),"application/vnd.ms-excel","" + "" + journal + "  от " + dateTime + ".xls"); 
            }
            else
                return null;
        }
	}
}