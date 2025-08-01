using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace suvarnyug.Models
{
    [Table("biodataimages")]
    public class BiodataImage
    {
        public int ImageId { get; set; }
        public int BiodataId { get; set; }
        public string ImageUrl { get; set; }
        public Biodata Biodata { get; set; }
        [Required]
        public bool IsDefault { get; set; } = false;
    }
}
