using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("cities")]
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int StateId { get; set; }
        public State State { get; set; }
    }
}
