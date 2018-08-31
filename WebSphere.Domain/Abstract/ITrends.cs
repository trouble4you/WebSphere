using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{

    public class MyTrend
    {
        public UInt64 id = 0;
        public List<Point> Points;
    }

    public class Point
    {
        public double v = 0;
        public UInt64 dt = 0;
    }
    public class MyTrendData
    {
        public double min = 0;
        public double max = 0;
        public UInt64 date_min_sec = 0;
        public UInt64 date_max_sec = 0;
        public List<Int32> signals = new List<Int32>();
        public List<MyTrend> trends = new List<MyTrend>();
    }
    public class MyTrendPage
    {
        public string object_type_str = "";
        public string object_name = "";

        public int object_id = 0;
        public UInt64 start_date = 0;
        public UInt64 end_date = 0;

        public List<OrderedDictionary> signals = new List<OrderedDictionary>();
        public List<Objects> objects = new List<Objects>();
    }


    public interface ITrends
    {
        MyTrendData GetTrend(string sd, string ed, string signs);
        MyTrendPage GetTrend(int id);
        List<MyTrend> GetTrendOpc(string signs);
    }

}
