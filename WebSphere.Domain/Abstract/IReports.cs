using System;
using System.Collections.Generic; 
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public class Report
    {
        public int Id;
        public string Name;
        public string StoredP;
        public List<string> Head; 
        public List<List<string>> Rows; 
    }
    public interface IReportServer
    {
        List<Report> ReportList();
        Report GetReport(int id, Dictionary<string, dynamic> parameters);
        MemoryStream GetExcel(int id, Dictionary<string, dynamic> parameters, out string journal);
    }
  
}
