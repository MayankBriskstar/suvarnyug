using Suvarnyug.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace suvarnyug.Models
{
    [Table("interestedprofiles")]
    public class InterestedProfile
    {
        public int Id { get; set; }
        public int InterestedUserId { get; set; }
        public int BiodataId { get; set; }
        public int InterestedBiodataId { get; set; }
        public DateTime InterestedAt { get; set; }

        public virtual Biodata Biodata { get; set; }

        //public virtual Biodata InterestedBiodata { get; set; }

        //public virtual User InterestedUser { get; set; }
    }

}
