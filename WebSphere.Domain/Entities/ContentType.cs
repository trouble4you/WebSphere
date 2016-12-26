using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class ContentType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Controller { get; set; }

        public List<Permission> Actions { get; set; }

        //public string GroupName { get; set; }

        public ContentGroup contentGroup { get; set; }

        public bool Selected { get; set; }
    }
}
