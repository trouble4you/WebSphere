using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSphere.Domain.Entities
{
    public class Alarm
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int TagId { get; set; }

        [Required]
        public int SRes { get; set; }

        [Required]
        public DateTime STime { get; set; }

        [Required]
        public float SVal { get; set; }

        public int? ERes { get; set; }

        public DateTime? ETime { get; set; }

        public float? EVal { get; set; }

        public DateTime? AckTime { get; set; }

        public int? Ack { get; set; }
    }
}

public class Event
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public int TagId { get; set; }

    [Required]
    public float Value { get; set; }

    [Required]
    public DateTime Time { get; set; }
     
     
}