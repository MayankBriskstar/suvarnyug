using Suvarnyug.Models;

namespace suvarnyug.Models
{
    public class ViewBiodataViewModel
    {
        public List<Biodata> SelfBiodata { get; set; }
        public List<Biodata> OnBehalfOfBiodata { get; set; }
        public List<ProfileViewHistory> ViewHistory { get; set; }
        public List<Biodata> InterestedProfiles { get; set; } // Add this
        public List<Biodata> ReceivedInterestProfiles { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        public List<(Biodata InterestedByBiodata, Biodata YourProfile)> InterestedProfilesWithYourProfile { get; set; }
    }
}
