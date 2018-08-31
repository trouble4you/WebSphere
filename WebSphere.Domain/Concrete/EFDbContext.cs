using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Entities;
using System.Data.Entity;

namespace WebSphere.Domain.Concrete
{
    public class EFDbContext : DbContext
    {
        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Event> Events { get; set; }

        public DbSet<Objects> Objects { get; set; }

        public DbSet<ObjectType> ObjectTypes { get; set; }

        public DbSet<Property> Properties { get; set; }

        public DbSet<PropType> PropTypes { get; set; }

        public DbSet<SignalsAnalog> SignalsAnalog { get; set; }

        public DbSet<SignalsDiscrete> SignalsDiscrete { get; set; }
    }
}
