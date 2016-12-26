using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public bool Selected { get; set; }

        public List<RolePermissionsTemplate> Permissions { get; set; }
    }
}