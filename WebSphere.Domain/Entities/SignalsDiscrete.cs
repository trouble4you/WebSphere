using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class SignalsDiscrete
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int TagId { get; set; }

        public bool Value { get; set; }

        public DateTime Datetime { get; set; }

        public DateTime RegTime { get; set; }
    }
}
