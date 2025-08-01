using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("states")]
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public ICollection<City> Cities { get; set; }
    }
}
