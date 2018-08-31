using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Abstract;

namespace WebSphere.Domain.Concrete
{
    public class JSON : IJSON
    {
        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public object Deserialize(string str, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(str, type);
        }

        public List<dynamic> Deserialize(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(str);
        }
        public object DeserializeObj(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(str);
        }
    }
}
