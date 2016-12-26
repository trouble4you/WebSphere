using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public int Type { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PasswordSalt { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string Email { get; set; }

        public int IsActive { get; set; }

        public int IsSuperuser { get; set; }

        public string LastLogin { get; set; }

        public string DateJoined { get; set; }

        public List<Role> Roles { get; set; }
    }
}
