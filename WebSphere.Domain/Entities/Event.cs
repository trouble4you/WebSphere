using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class Event
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int TagId { get; set; } 

        [Required]
        public DateTime STime { get; set; }

        [Required]
        public float SVal { get; set; }
         
         
         
    }
}
