using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class ControllerAction
    {
        public string Controller { get; set; }

        public List<string> Actions { get; set; }
    }
}
