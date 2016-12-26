using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Abstract
{
    public interface IJSON
    {
        string Serialize(object obj);

        object Deserialize(string str, Type type);

        List<dynamic> Deserialize(string str);
    }
}
