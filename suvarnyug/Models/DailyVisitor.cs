using System;
using System.ComponentModel.DataAnnotations;

namespace Suvarnyug.Models
{
    public class DailyVisitor
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        public DateTime VisitDateTime { get; set; }
    }
}
