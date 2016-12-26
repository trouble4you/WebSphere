using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class RolePermissionsTemplate
    {
        // контроллер
        public string Name { get; set; }

        // экшены
        public List<Permission> Permission { get; set; }
    }
}
