using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MnemoController : Controller
    {
        private bool ViewExists(string name)
        {
            ViewEngineResult result = ViewEngines.Engines.FindView(ControllerContext, name, null);
            return (result.View != null);
        }

        private EFDbContext context = new EFDbContext();

        private IOpcPoller _opcPoller;

        //   public ActionResult ZD_Shkapovo(int id)
        //   {   // if (!MyUser.CheckPermission(PermissionRight.ConfUser))
        //       // {
        //       //     MyTpl.TplCommonData(ViewBag);
        //       //     return View("ConfiguratorPermFail", (object)"Нет доступа к конфигуратору пользователей");
        //       // }
        //       //var users = new WebTelemetrySphere.Models.MyUsers(); 
        //   var zdv1 = context.Objects.Select(c => c.Id==id).FirstOrDefault();

        //       var taglist=new List<string>();
        //       var rezult = from Object in context.Objects
        //           join to in context.Properties on Object.Id equals to.ObjectId  
        //           where Object.ParentId == id && Object.Type == 2 
        //           select to.Value;
        //       foreach (string c in rezult)
        //       {
        //           dynamic dict = _json.DeserializeObject(c);
        //           if (dict != null)
        //           { 
        //               taglist.Add(Convert.ToString(dict.Connection)); 
        //           }
        //       }
        //  var zdv= new ZdvModel{Id = 1,Name = "1",Tags =taglist };
        ////             Dictionary<string, dynamic> dict = null;
        ////             foreach (string c in custs)
        ////             {

        //      // MySQLResult r = MyDB.sql_query_local("SELECT name FROM Objects WHERE id = " + zdv.Id);
        //       //zdv.Name = r.GetValue(0, "name");
        //       //return View("UsersConfigurator", users);
        //       return View("ZD_Skapovo", zdv);
        //   }

        public ActionResult MnemoObj(string mnemo, string objname)
        {

            var obj = context.Objects.FirstOrDefault(c => c.Name == objname);
            if (obj != null && ViewExists(mnemo))
            {
                var zdv = new Obj { Id = obj.Id, Name = obj.Name, Type = obj.Type.ToString() };
                return View(mnemo, zdv);
            }
            else
            {
                return RedirectToAction("Error", "Error_404");
            } 
        }
        public ActionResult MnemoRK(string mnemo, string objname)
        {

            var obj = context.Objects.FirstOrDefault(c => c.Name == objname);
            if (obj != null && ViewExists(mnemo))
            {
                var zdv = new Obj { Id = obj.Id, Name = obj.Name, Type = obj.Type.ToString() };
                return View(mnemo, zdv);
            }
            else
            {
                return RedirectToAction("Error", "Error_404");
            } 
        }

        public ActionResult Mnemo(string mnemo)
        {
            if (ViewExists(mnemo))
                return View(mnemo);
            else
                return RedirectToAction("Error", "Error_404");
        }

        public ActionResult RK()
        {   // if (!MyUser.CheckPermission(PermissionRight.ConfUser))
            // {
            //     MyTpl.TplCommonData(ViewBag);
            //     return View("ConfiguratorPermFail", (object)"Нет доступа к конфигуратору пользователей");
            // }
            //var users = new WebTelemetrySphere.Models.MyUsers(); 
            // var zdv1 = context.Objects.Select(c => c.Id == id).FirstOrDefault();
            //
            // var taglist = new List<string>();
            // var rezult = from Object in context.Objects
            //              join to in context.Properties on Object.Id equals to.ObjectId
            //              where Object.ParentId == id && Object.Type == 2
            //              select to.Value;
            // foreach (string c in rezult)
            // {
            //     dynamic dict = JsonConvert.DeserializeObject(c);
            //     if (dict != null)
            //     {
            //         taglist.Add(Convert.ToString(dict.Connection));
            //     }
            // }
            // var zdv = new ZdvModel { Id = 1, Name = "1", Tags = taglist };
            //             Dictionary<string, dynamic> dict = null;
            //             foreach (string c in custs)
            //             {
            // MySQLResult r = MyDB.sql_query_local("SELECT name FROM Objects WHERE id = " + zdv.Id);
            //zdv.Name = r.GetValue(0, "name");
            //return View("UsersConfigurator", users);
            return View();
        }

        private readonly object _readOpcLocker = new object();
    }
}