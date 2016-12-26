using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class Property
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int ObjectId { get; set; }

        [Required]
        public int PropId { get; set; }

        public string Value { get; set; }
    }
}
