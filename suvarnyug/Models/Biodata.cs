using suvarnyug.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Suvarnyug.Models
{
    [Table("biodata")]
    public class Biodata
    {
        public int BiodataId { get; set; }
        [Required]
        public int UserId { get; set; }

        [Display(Name = "Biodata For")]
        [Required(ErrorMessage = "Please Specify Whether This Biodata Is For MySelf Or On BeHalf Of Someone.")]
        public bool IsForSelf { get; set; }

        [Display(Name = "On Behalf Of")]
        [Required(ErrorMessage = "Please Specify BeHalf Of.")]
        public string? BehalfOf { get; set; }

        [Required(ErrorMessage = "Please Enter First Name")]
        [StringLength(50, ErrorMessage = "First Name Cannot Be Longer Than 50 Characters")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [StringLength(50, ErrorMessage = "Last Name Cannot Be Longer Than 50 Characters")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Please Select Date Of Birth")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Please Enter Time Of Birth")]
        [DataType(DataType.Time)]
        [Display(Name = "Time of Birth")]
        public TimeSpan? TimeOfBirth { get; set; }

        [Required(ErrorMessage = "Please Enter Place Of Birth")]
        [StringLength(50, ErrorMessage = "Place Of Birth Cannot Be Longer Than 50 Characters")]
        [Display(Name = "Place of Birth")]
        public string? PlaceOfBirth { get; set; }

        [Required(ErrorMessage = "Please Select Gender")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please Select Height")]
        public string Height { get; set; }

        [Required(ErrorMessage = "Please Enter Weight")]
        [Range(0, 500, ErrorMessage = "Please Enter A Valid Weight")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Please Select Physical Status")]
        [StringLength(100, ErrorMessage = "Physical Status cannot be longer than 100 characters")]
        public string? PhysicalStatus { get; set; }

        [Required(ErrorMessage = "Please Select Country")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Please Select State")]
        public int StateId { get; set; }

        [Required(ErrorMessage = "Please Select City")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Please Select at least one Education")]
        public string Education { get; set; }
        [NotMapped]
        public List<string> SelectedEducationList
        {
            get => string.IsNullOrEmpty(Education) ? new List<string>() : Education.Split(',').ToList();
            set => Education = string.Join(",", value);
        }

        [Required(ErrorMessage = "Please Enter Caste")]
        [StringLength(50, ErrorMessage = "Caste Cannot Be Longer Than 50 Characters")]
        public string? Cast { get; set; }

        [Required(ErrorMessage = "Please Enter Sub Caste")]
        [StringLength(100, ErrorMessage = "Subcaste cannot be longer than 100 characters")]
        public string? Subcaste { get; set; }

        [Required(ErrorMessage = "Please Select Income")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please Select Income")]
        public string Income { get; set; }

        [Required(ErrorMessage = "Please Select Marital Status")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please Select Marital Status")]
        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }

        [Required(ErrorMessage = "Please Enter Father's Name")]
        [StringLength(50, ErrorMessage = "Father's Name Cannot Be Longer Than 50 Characters")]
        [Display(Name = "Father's Name")]
        public string? FatherName { get; set; }

        [Required(ErrorMessage = "Please Enter Father's Status")]
        [StringLength(100, ErrorMessage = "Father's Status cannot be longer than 100 characters")]
        [Display(Name = "Father's Status")]
        public string? FathersStatus { get; set; }

        [Required(ErrorMessage = "Please Enter Mother's Name")]
        [StringLength(50, ErrorMessage = "Mother's Name Cannot Be Longer Than 50 Characters")]
        [Display(Name = "Mother's Name")]
        public string? MotherName { get; set; }

        [Required(ErrorMessage = "Please Enter Mother's Status")]
        [StringLength(100, ErrorMessage = "Mother's Status cannot be longer than 100 characters")]
        [Display(Name = "Mother's Status")]
        public string? MothersStatus { get; set; }

        [Range(0, 10, ErrorMessage = "Please Select A Valid Number Of Brothers.")]
        [Required(ErrorMessage = "Please Select The Number Of Brothers.")]
        [Display(Name = "Number of Brothers")]
        public int NumberOfBrothers { get; set; }

        [Range(0, 10, ErrorMessage = "Please Select A Valid Number Of Sisters.")]
        [Required(ErrorMessage = "Please Select The Number Of Sisters.")]
        [Display(Name = "Number of Sisters")]
        public int NumberOfSisters { get; set; }

        //[Required(ErrorMessage = "Please Enter Mobile Number")]
        //[RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid Mobile Number. It Should Be Eactly 10 Digits.")]
        //[Phone]
        //[Display(Name = "Mobile Number")]
        //public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Please Enter EmailId")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100, ErrorMessage = "Email Cannot Be Longer Than 100 Characters")]
        public string? Email { get; set; }

        //[Required(ErrorMessage = "Please Enter Address")]
        //[StringLength(250, ErrorMessage = "Address Cannot Be Longer Than 250 Characters")]
        //public string? Address { get; set; }

        [Required(ErrorMessage = "Please Select House Type")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Please Select House Type")]
        [Display(Name = "House Type")]
        public string HouseType { get; set; }

        [Required(ErrorMessage = "Please Select Mother Tongue")]
        [Display(Name = "Mother Tongue")]
        public string MotherTongue { get; set; }

        [Required(ErrorMessage = "Please Enter Partner's Marital Status")]
        [StringLength(50, ErrorMessage = "Partner's Marital Status cannot be longer than 50 characters")]
        [Display(Name = "Partner's Marital Status")]
        public string PartnerMaritalStatus { get; set; }

        [Required(ErrorMessage = "Please Enter Partner's Education")]
        [StringLength(100, ErrorMessage = "Partner's Education cannot be longer than 100 characters")]
        [Display(Name = "Partner's Education")]
        public string PartnerEducation { get; set; }

        [Required(ErrorMessage = "Please Enter Partner's Caste")]
        [StringLength(100, ErrorMessage = "Partner's Caste cannot be longer than 100 characters")]
        [Display(Name = "Partner's Caste")]
        public string PartnerCast { get; set; }

        [Required(ErrorMessage = "Please Enter Partner's Country")]
        [StringLength(100, ErrorMessage = "Partner's Country cannot be longer than 100 characters")]
        [Display(Name = "Partner's Country")]
        public string PartnerCountry { get; set; }

        [Required(ErrorMessage = "Please Enter About My Partner")]
        [StringLength(1000, ErrorMessage = "About your partner cannot be longer than 500 characters")]
        [Display(Name = "About My Partner")]
        public string AboutMyPartner { get; set; }

        public ICollection<BiodataImage> Images { get; set; }
        public Country Country { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public bool IsDeleted { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; }
        public ICollection<ProfileViewHistory> ProfileViews { get; set; } = new List<ProfileViewHistory>();
        public int ViewCount => ProfileViews?.Count ?? 0;
        public BiodataImage DefaultImage => Images?.FirstOrDefault(img => img.IsDefault) ?? Images?.FirstOrDefault();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsForSelf)
            {
                if (string.IsNullOrWhiteSpace(BehalfOf))
                {
                    yield return new ValidationResult("Please specify on behalf of whom.", new[] { "BehalfOf" });
                }
            }
        }
      
    }
}