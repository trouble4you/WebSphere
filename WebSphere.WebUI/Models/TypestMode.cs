using System.Collections.Specialized;
using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.WebUI.Models
{
    public class MyTrendPage
    {
        public string object_type_str = "";
        public string object_name = "";

        public int object_id = 0;
        public UInt64 start_date = 0;
        public UInt64 end_date = 0;

        public Dictionary<int, string> Objects = new Dictionary<int, string>();
        public List<OrderedDictionary> signals = new List<OrderedDictionary>();
        public string JsTree;
    }
    public class Trend
    {
        public UInt64 start_date = 0;
        public UInt64 end_date = 0;

        public List<Obj> Objects = new List<Obj>();
        public List<Signal> Signals = new List<Signal>();
    }
    public class Obj
    {
        public int Id;
        public string Name;
        public string Type;
    }
    public class Well
    {
        public int Id;
        public string Name;
        public string FullName;
        public string Type;
        public string Port;
    }
    public class AGZU
    {
        public int Id;
        public string Name;
        public string FullName;
        public string Type;
        public string Port;
    }
    public class Signal
    {
        public int Id;
        public string Name;
    }
}