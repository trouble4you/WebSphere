using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Script.Serialization;
using WebSphere.Domain.Abstract;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Controllers.API
{
    [Authorize]
    public class ReportController : ApiController
    {
        private   Logging logger = new Logging(); 

        private EFDbContext context = new EFDbContext();

        public class Object
        {
            public Int32 Id { get; set; }

            public string Name { get; set; }

        }


        public dynamic GetClassType(string report)
        {
            switch (report)
            {   case "Report1":  return  new List<Object>(); 
                case "Report2":  return  new List<Object>();
                default: return null;
            }
            
                      return null;
        }

    [HttpPost]
        public List<Object> GetReport(FormDataCollection data)
        {
        //(string name, List<string> parameters)


            var Report = data.Get("Report");

            var rezultList = GetClassType(Report);
            var rezultList1 = new List<Object>();
            try
            { 
                using (context)
                {            object[] parameters = 
                        {  
                            new SqlParameter("@Id", "NULL") ,
                            new SqlParameter("@Name", "NULL") 
                        };
                var rez = context.Database.SqlQuery<Object>("GetObjects @Id,@Name", parameters);
                    //var s = context.ExecuteStoreQuery<List<string>>("exec " + storedProcedure).ToList();
                    rezultList = rez.ToList();
                    //context.ExecuteStoreQuery<Product>("select * from Products where pid = {0}", 1);
                  
                   // var report = DbSet.SqlQuery(Report) ;
                     
                        //rezultList.Add(report); 
                }

            }
            catch (Exception ex)
            {
                return rezultList;
            }
            return rezultList;
        }
    
    }
}
