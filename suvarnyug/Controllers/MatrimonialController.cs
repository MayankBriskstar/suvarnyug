using AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using suvarnyug.Models;
using Suvarnyug.Data;
using Suvarnyug.Models;
using System.Security.Claims;
using DinkToPdf;
using DinkToPdf.Contracts;
using PuppeteerSharp;
using Microsoft.ML;
using Microsoft.ML.Data;
using PuppeteerSharp.Media;
using System.Globalization;
using System.Text.RegularExpressions;
namespace suvarnyug.Controllers
{
    public class MatrimonialController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MatrimonialController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ViewProfile(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var biodata = await _context.Biodata
                .Include(b => b.Images)
                .Include(b => b.Country).Include(b => b.State).Include(b => b.City)
                .Where(b => b.IsPremiumActive == false)
                .FirstOrDefaultAsync(b => b.BiodataId == id && b.UserId.ToString() == userId);

            if (biodata == null)
            {
                return RedirectToAction("notfound", "shared");
            }
            return View(biodata);
        }

        [Authorize]
        [HttpGet]
        public IActionResult InterestedProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("login", "account");
            }
            var userId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            //if (user.Role != "Admin")
            //{
            //    var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.Now);

            //    if (subscription == null || (subscription.PlanType != PlanType.Premium))
            //    {
            //        return RedirectToAction("SubscriptionDetails", "Payment");
            //    }        
            //}
            var receivedInterestProfiles = _context.InterestedProfiles
                .Where(ip => ip.InterestedUserId == userId && ip.Biodata.IsDeleted == false && !ip.Biodata.User.IsActive)
                .Include(ip => ip.Biodata)
                .ThenInclude(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Include(ip => ip.Biodata.Country)
                .ToList();

            var receivedBiodata = receivedInterestProfiles
                .Select(ip => (InterestedByBiodata: ip.Biodata, YourProfile: _context.Biodata
                .Include(b => b.Country)
                .Include(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Where(b => b.IsPremiumActive == false)
                .FirstOrDefault(b => b.BiodataId == ip.InterestedBiodataId))).ToList();

            var model = new ViewBiodataViewModel
            {
                InterestedProfilesWithYourProfile = receivedBiodata.Where(x => x.InterestedByBiodata != null && x.YourProfile != null).ToList()
            };

            return View(model);

        }

        [Authorize]
        [HttpGet]
        [Route("matrimonial/viewinterestedprofile/{interestedBiodataId}")]
        public async Task<IActionResult> ViewInterestedProfile(int interestedBiodataId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
            //if (user.Role != "Admin")
            //{
            //    var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == int.Parse(userId) && s.IsActive && s.EndDate > DateTime.Now);

            //    if (subscription == null || (subscription.PlanType != PlanType.Premium))
            //    {
            //        return RedirectToAction("SubscriptionDetails", "Payment");
            //    }
            //}
            var interestedProfile = await _context.InterestedProfiles
                .Include(ip => ip.Biodata)
                .ThenInclude(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Include(ip => ip.Biodata.Country)
                .Include(ip => ip.Biodata.State)
                .Include(ip => ip.Biodata.City)
                .FirstOrDefaultAsync(ip =>
                    ip.BiodataId == interestedBiodataId &&
                    ip.InterestedUserId == int.Parse(userId));

            if (interestedProfile == null)
            {
                return RedirectToAction("NotFound", "Shared");
            }

            return View("viewinterestedprofile", interestedProfile.Biodata);
        }

        [Authorize]
        public IActionResult FindPartner(string gender = null, string income = null, string maritalStatus = null, string country = null, int? minAge = null, int? maxAge = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Countries = _context.Countries.ToList();
            var allBiodata = _context.Biodata.Include(b => b.User)
                .ThenInclude(u => u.UserLogin)
                .Include(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Where(b => b.IsDeleted == false && !b.User.IsActive)
                .AsQueryable();

            var loggedInUserId = int.Parse(userId);
            var user = _context.Users.FirstOrDefault(u => u.UserId == loggedInUserId);
            var subscription = _context.Subscriptions
                    .FirstOrDefault(s => s.UserId == loggedInUserId && s.IsActive && s.EndDate > DateTime.Now);
            bool hasPremium = subscription != null && subscription.PlanType == PlanType.Premium;

            if (!string.IsNullOrEmpty(userId))
            {
                allBiodata = allBiodata.Where(b => b.UserId.ToString() != userId);
            }
            if (!hasPremium && user.Role != "Admin")
            {
                allBiodata = allBiodata.Where(b => b.Gender == "Male");
            }
            ViewBag.TotalOrFilteredCount = allBiodata.Count();
            if (!string.IsNullOrEmpty(gender))
            {
                allBiodata = allBiodata.Where(b => b.Gender == gender);
            }
            if (!string.IsNullOrEmpty(maritalStatus))
            {
                allBiodata = allBiodata.Where(b => b.MaritalStatus == maritalStatus);
            }
            if (!string.IsNullOrEmpty(income))
            {
                allBiodata = allBiodata.Where(b => b.Income == income);
            }
            if (!string.IsNullOrEmpty(country))
            {
                allBiodata = allBiodata.Where(b => b.Country.CountryName.Contains(country));
            }

            if (minAge.HasValue || maxAge.HasValue)
            {
                DateTime currentDate = DateTime.Now;

                if (minAge.HasValue)
                {
                    var minDateOfBirth = currentDate.AddYears(-minAge.Value);
                    allBiodata = allBiodata.Where(b => b.DOB <= minDateOfBirth);
                }

                if (maxAge.HasValue)
                {
                    var maxDateOfBirth = currentDate.AddYears(-maxAge.Value);
                    allBiodata = allBiodata.Where(b => b.DOB >= maxDateOfBirth);
                }
            }
            ViewBag.TotalOrFilteredCount = allBiodata.Count();
            allBiodata = allBiodata.Where(b => b.VipBiodata == false).OrderByDescending(b => b.CreatedAt);
            return View(allBiodata.ToList());
        }

        #region Find patner using text
        [Authorize]
        public async Task<IActionResult> FindPartnerAISearch(string query = "")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
            if (user.Role != "Admin")
            {
                var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == int.Parse(userId) && s.IsActive && s.EndDate > DateTime.Now);

                if (subscription == null || subscription.PlanType != PlanType.Premium)
                {
                    return RedirectToAction("SubscriptionDetails", "Payment");
                }
            }
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            var filters = ExtractFiltersFromText(query, _context);

            var allProfiles = _context.Biodata
                .Include(b => b.User.UserLogin)
                .Include(b => b.Images)
                .Include(b => b.City).ThenInclude(c => c.State).ThenInclude(s => s.Country)
                .Where(b => b.UserId.ToString() != userId && !b.IsDeleted && !b.User.IsActive && b.VipBiodata == false)
                .AsQueryable();
            if (!string.IsNullOrEmpty(filters.Gender))
            {
                var genderList = filters.Gender.Split(',').Select(s => s.Trim()).ToList();
                allProfiles = allProfiles.Where(b => genderList.Contains(b.Gender));
            }
            if (!string.IsNullOrEmpty(filters.MaritalStatus))
            {
                var statusList = filters.MaritalStatus.Split(',').Select(s => s.Trim()).ToList();
                allProfiles = allProfiles.Where(b => statusList.Contains(b.MaritalStatus));
            }
            if (!string.IsNullOrEmpty(filters.MinIncome))
            {
                var incomeList = filters.MinIncome.Split(',').Select(i => i.Trim()).ToList();
                allProfiles = allProfiles.Where(b => incomeList.Contains(b.Income));
            }
            if (filters.Countries.Any())
            {
                allProfiles = allProfiles.Where(b => filters.Countries.Contains(b.Country.CountryName));
            }
            if (filters.States.Any())
            {
                allProfiles = allProfiles.Where(b => filters.States.Contains(b.City.State.StateName));
            }
            if (filters.Cities.Any())
            {
                allProfiles = allProfiles.Where(b => filters.Cities.Contains(b.City.CityName));
            }
            if (filters.Names.Any())
            {
                allProfiles = allProfiles.Where(b => filters.Names.Contains(b.FirstName) || filters.Names.Contains(b.LastName));
            }
            var recommendedProfiles = allProfiles.OrderByDescending(b => b.CreatedAt).ToList();

            ViewBag.TotalOrFilteredCount = recommendedProfiles.Count();
            ViewBag.Countries = _context.Countries.ToList();
            ViewBag.IsAISuggestion = true;

            return View("FindPartner", recommendedProfiles);
        }
        private FilterCriteria ExtractFiltersFromText(string query, ApplicationDbContext _context)
        {
            var filters = new FilterCriteria();
            query = query.ToLower();

            List<string> Genderboth = new List<string>();
            if (query.Contains("bride") || query.Contains("female") || query.Contains("girl"))
                Genderboth.Add("Female");
            if (query.Contains("groom") || query.Contains("male") || query.Contains("men") || query.Contains("boy"))
                Genderboth.Add("Male");
            filters.Gender = string.Join(", ", Genderboth);

            List<string> matchedStatuses = new List<string>();
            if (query.Contains("unmarried") || query.Contains("single") || query.Contains("notmarried"))
                matchedStatuses.Add("Un-Married");
            if (query.Contains("divorced") || query.Contains("divorcee"))
                matchedStatuses.Add("Divorcee");
            if (query.Contains("widowed") || query.Contains("widow"))
                matchedStatuses.Add("Widow");
            filters.MaritalStatus = string.Join(", ", matchedStatuses);

            List<string> matchedIncomes = new List<string>();
            if (query.Contains("1lac") || query.Contains("under1lac") || query.Contains("100000"))
                matchedIncomes.Add("Under 1 Lac");
            if (query.Contains("2lac") || query.Contains("200000") || query.Contains("100000") || query.Contains("1-2 lac"))
                matchedIncomes.Add("1-2 Lac");
            if (query.Contains("2lac") || query.Contains("200000") || query.Contains("3lac") || query.Contains("4lac") || query.Contains("5lac") ||
                query.Contains("500000") || query.Contains("300000") || query.Contains("400000") || query.Contains("2-5 lac"))
                matchedIncomes.Add("2-5 Lac");
            if (query.Contains("5lac") || query.Contains("500000") || query.Contains("6lac") || query.Contains("7lac") || query.Contains("8lac") ||
                query.Contains("9lac") || query.Contains("10lac") || query.Contains("600000") || query.Contains("700000") ||
                query.Contains("800000") || query.Contains("900000") || query.Contains("1000000"))
                matchedIncomes.Add("5-10 Lac");
            if (query.Contains("10lac") || query.Contains("above10lac") || query.Contains("1000000"))
                matchedIncomes.Add("Over 10 Lac");
            filters.MinIncome = string.Join(", ", matchedIncomes);

            //var incomeList = _context.Biodata
            //.Where(u => !string.IsNullOrEmpty(u.Income))
            //.Select(u => u.Income)
            //.Distinct()
            //.ToList();

            //foreach (var income in incomeList)
            //{
            //    if (query.Contains(income.ToLower()))
            //    {
            //        filters.MinIncome = income; // Assign the matched income category
            //        break;
            //    }
            //}

            //var countryList = _context.Countries.Select(c => c.CountryName.ToLower()).ToList();
            //filters.Countries = countryList.Where(c => query.Contains(c)).Distinct().ToList();
            //var countryNames = _context.Countries.Select(c => c.CountryName.ToLower()).ToList();
            //var detectedCountries = new List<string>();
            //foreach (var country in countryNames)
            //{
            //    if (query.Contains(country) || country.Split(' ').Any(word => query.Contains(word)))
            //    {
            //        detectedCountries.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country));
            //    }
            //}
            //filters.Countries = detectedCountries.Distinct().ToList();
            var countryList = _context.Countries.Select(s => s.CountryName.ToLower()).ToList();
            var detectedCountry = countryList.Where(s => s.Contains(query)).Distinct().ToList();
            filters.Countries = detectedCountry;
            //state
            var stateList = _context.States.Select(s => s.StateName.ToLower()).ToList();
            var detectedStates = stateList.Where(s => s.Contains(query)).Distinct().ToList();
            filters.States = detectedStates;

            // City Filter
            var cityList = _context.Cities.Select(ci => ci.CityName.ToLower()).ToList();
            var detectedCities = cityList.Where(ci => ci.Contains(query)).Distinct().ToList();
            filters.Cities = detectedCities;

            var nameParts = query.Split(" ");
            var allNames = _context.Biodata.Select(b => new { b.FirstName, b.LastName }).ToList();

            foreach (var name in allNames)
            {
                if (!string.IsNullOrEmpty(name.FirstName) && nameParts.Contains(name.FirstName.ToLower()))
                    filters.Names.Add(name.FirstName);

                if (!string.IsNullOrEmpty(name.LastName) && nameParts.Contains(name.LastName.ToLower()))
                    filters.Names.Add(name.LastName);
            }

            return filters;
        }

        public class FilterCriteria
        {
            public string Gender { get; set; }
            public string MaritalStatus { get; set; }
            public List<string> Countries { get; set; } = new List<string>();
            public List<string> States { get; set; } = new List<string>();
            public List<string> Cities { get; set; } = new List<string>();
            public string MinIncome { get; set; }
            public List<string> Names { get; set; } = new List<string>();
        }

        #endregion

        #region Find patner using button
        [Authorize]
        public async Task<IActionResult> FindPartnerAI()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
            if (user.Role != "Admin")
            {
                var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == int.Parse(userId) && s.IsActive && s.EndDate > DateTime.Now);

                if (subscription == null || subscription.PlanType != PlanType.Premium)
                {
                    return RedirectToAction("SubscriptionDetails", "Payment");
                }
            }
            var userProfiles = _context.Biodata
                .Include(b => b.User)
                .Include(b => b.City).ThenInclude(c => c.State).ThenInclude(s => s.Country)
                .Where(b => b.UserId.ToString() == userId && !b.IsDeleted)
                .ToList();

            if (!userProfiles.Any())
            {
                return RedirectToAction("createprofile", "matrimonial");
            }

            var allProfiles = _context.Biodata
                .Include(b => b.User.UserLogin)
                .Include(b => b.Images)
                .Include(b => b.Country)
                .Where(b => b.UserId.ToString() != userId && !b.IsDeleted && b.VipBiodata == false)
                .AsEnumerable()
                .Where(b => userProfiles.Any(up => up.Gender != b.Gender && !b.User.IsActive &&
                                                   (up.CountryId == b.CountryId || up.StateId == b.StateId)))
                .ToList();


            var userProfileTexts = userProfiles.Select(p => GetProfileText(p)).ToList();
            var profileTexts = allProfiles.Select(p => GetProfileText(p)).ToList();
            var similarityScores = userProfileTexts
                .SelectMany(userText => CalculateTextSimilarity(userText, profileTexts))
                .ToList();
            var recommendedProfiles = allProfiles
                .Select((profile, index) => new { Profile = profile, Score = similarityScores.ElementAtOrDefault(index) })
                .Where(p => p.Score != null)
                .OrderByDescending(p => p.Score)
                .Select(p => p.Profile)
                .ToList();

            ViewBag.TotalOrFilteredCount = recommendedProfiles.Count();
            ViewBag.Countries = _context.Countries.ToList();
            ViewBag.IsAISuggestion = true;
            return View("FindPartner", recommendedProfiles);
        }

        private static string GetProfileText(Biodata profile)
        {
            return $"{profile.FirstName} {profile.LastName} {profile.Income} {profile.MotherTongue} " +
                   $"{profile.Country.CountryId} {profile.StateId} {profile.DOB.Year}";
        }
        public static List<double> CalculateTextSimilarity(string userProfile, List<string> allProfiles)
        {
            var mlContext = new MLContext();

            var allTexts = new List<string> { userProfile }.Concat(allProfiles).ToList();

            var data = allTexts.Select(text => new TextData { Text = text }).ToList();
            var trainData = mlContext.Data.LoadFromEnumerable(data);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(TextData.Text));
            var model = pipeline.Fit(trainData);
            var transformedData = model.Transform(trainData);

            var vectors = mlContext.Data.CreateEnumerable<TransformedData>(transformedData, reuseRowObject: false)
                                        .Select(d => d.Features)
                                        .ToList();

            var userVector = vectors[0];

            var similarities = vectors.Skip(1).Select(profileVector => CosineSimilarity(userVector, profileVector)).ToList();

            return similarities;
        }
        private static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            double dotProduct = 0, magA = 0, magB = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magA += Math.Pow(vectorA[i], 2);
                magB += Math.Pow(vectorB[i], 2);
            }
            return dotProduct / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
        public class TextData
        {
            public string Text { get; set; }
        }
        public class TransformedData : TextData
        {
            public float[] Features { get; set; }
        }

        #endregion

        [HttpGet]
        public async Task<JsonResult> GetStates(int id)
        {
            var states = await _context.States.Where(s => s.CountryId == id).ToListAsync();
            return Json(states);
        }

        [HttpGet]
        public async Task<JsonResult> GetCities(int id)
        {
            var cities = await _context.Cities.Where(c => c.StateId == id).ToListAsync();
            return Json(cities);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CreateProfile()
        {
            var biodata = new Biodata();
            ViewBag.Countries = await _context.Countries.ToListAsync();
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //int userId = int.Parse(userIdClaim.Value);
            //var user = await _context.Users.FindAsync(userId);
            //if (user == null || !user.IsVerified)
            //{
            //    return RedirectToAction("UploadDocument", "Account");
            //}
            return View(biodata);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProfile(Biodata biodata, IFormFile[] images, string behalfOf, int isForSelf, int? defaultImageIndex, IFormCollection form)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("login", "account");
            }

            biodata.UserId = int.Parse(userIdClaim.Value);
            bool isForSelfBool = isForSelf == 1;
            biodata.IsForSelf = isForSelfBool;
            biodata.CreatedAt = DateTime.Now;
            var selectedEducation = form["Education"];
            biodata.Education = string.Join(",", selectedEducation);
            var loggedInUserId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == loggedInUserId);
            if (user.Role != "Admin")
            {
                var subscription = _context.Subscriptions
                    .FirstOrDefault(s => s.UserId == loggedInUserId && s.IsActive && s.EndDate > DateTime.Now);
                bool hasPremium = subscription != null && subscription.PlanType == PlanType.Premium;

                if (isForSelfBool)
                {
                    var existingBiodata = await _context.Biodata
                        .FirstOrDefaultAsync(b => b.UserId == biodata.UserId && b.IsForSelf && !b.IsDeleted);

                    if (existingBiodata != null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "You Have Already Created Matrimonial Profile For 'MySelf'. Only One Profile Is Allowed.");
                        ViewBag.Countries = await _context.Countries.ToListAsync();
                        return View(biodata);
                    }

                    biodata.BehalfOf = "self";

                    if (biodata.Gender?.ToLower() == "male" && !hasPremium)
                    {
                        ModelState.AddModelError(string.Empty,
                            "You cannot create a male profile for yourself without a Premium subscription.");
                        ViewBag.Countries = await _context.Countries.ToListAsync();
                        return View(biodata);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(behalfOf))
                    {
                        biodata.BehalfOf = behalfOf.ToLower();

                        if (biodata.BehalfOf == "daughter" || biodata.BehalfOf == "sister")
                        {
                            if (biodata.Gender?.ToLower() != "female")
                            {
                                ModelState.AddModelError(string.Empty,
                                    "Only female profiles are allowed for Daughter or Sister.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(biodata);
                            }
                        }
                        else if (biodata.BehalfOf == "friend")
                        {
                            if (biodata.Gender?.ToLower() == "male" && !hasPremium)
                            {
                                ModelState.AddModelError(string.Empty,
                                    "You cannot create a male profile for a Friend without a Premium subscription.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(biodata);
                            }
                        }
                        else if (biodata.BehalfOf == "son" || biodata.BehalfOf == "brother")
                        {
                            if (!hasPremium)
                            {
                                ModelState.AddModelError(string.Empty,
                                    $"You cannot create a profile for {biodata.BehalfOf} without a Premium subscription.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(biodata);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty,
                                "You can only create profiles on behalf of Daughter, Sister, Friend, Son, or Brother.");
                            ViewBag.Countries = await _context.Countries.ToListAsync();
                            return View(biodata);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,
                            "BehalfOf is required when creating for others.");
                        ViewBag.Countries = await _context.Countries.ToListAsync();
                        return View(biodata);
                    }
                }
            }
            var lastProfile = await _context.Biodata
                .Where(b => b.ProfileId.StartsWith("SY-"))
                .OrderByDescending(b => b.ProfileId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastProfile != null)
            {
                string numberPart = lastProfile.ProfileId.Substring(3);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            biodata.ProfileId = $"SY-{nextNumber:D4}";
            _context.Biodata.Add(biodata);
            await _context.SaveChangesAsync();

            if (images != null && images.Length > 0 && images.Length <= 5)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/databio");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                List<BiodataImage> biodataImages = new List<BiodataImage>();

                for (int i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var biodataImage = new BiodataImage
                    {
                        BiodataId = biodata.BiodataId,
                        ImageUrl = Path.Combine("images/databio", fileName),
                        IsDefault = (defaultImageIndex.HasValue && defaultImageIndex.Value == i)
                    };

                    biodataImages.Add(biodataImage);
                }
                _context.BiodataImages.AddRange(biodataImages);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("dashboard", "home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var biodata = await _context.Biodata
                .Include(b => b.Images)
                .Where(b => b.IsPremiumActive == false)
                .FirstOrDefaultAsync(b => b.BiodataId == id && b.UserId.ToString() == userId);
            ViewBag.Countries = await _context.Countries.ToListAsync();
            if (biodata == null)
            {
                return RedirectToAction("notfound", "shared");
            }

            return View(biodata);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(Biodata biodata, List<IFormFile> images, string behalfOf, int isForSelf, int? defaultImageIndex, IFormCollection form)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingBiodata = await _context.Biodata
               .Include(b => b.Images)
               .FirstOrDefaultAsync(b => b.BiodataId == biodata.BiodataId && b.UserId.ToString() == userId);

            if (existingBiodata == null)
            {
                return NotFound();
            }
            bool isForSelfBool = isForSelf == 1;
            if (isForSelfBool && !existingBiodata.IsForSelf)
            {
                var anotherSelfBiodata = await _context.Biodata
                    .FirstOrDefaultAsync(b => b.UserId == int.Parse(userId) && b.BiodataId != biodata.BiodataId && b.IsForSelf == true);

                if (anotherSelfBiodata != null)
                {
                    ModelState.AddModelError(string.Empty, "You Have Already Created Matrimonial Profile For 'MySelf'. Only One Profile Is Allowed.");
                    ViewBag.Countries = await _context.Countries.ToListAsync();
                    return View(existingBiodata);
                }
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));

            if (user.Role != "Admin")
            {
                var subscription = _context.Subscriptions
                    .FirstOrDefault(s => s.UserId == int.Parse(userId) && s.IsActive && s.EndDate > DateTime.Now);
                bool hasPremium = subscription != null && subscription.PlanType == PlanType.Premium;

                if (isForSelfBool)
                {
                    biodata.BehalfOf = "self";

                    if (biodata.Gender?.ToLower() == "male" && !hasPremium)
                    {
                        ModelState.AddModelError(string.Empty,
                            "You cannot edit a male profile for yourself without a Premium subscription.");
                        ViewBag.Countries = await _context.Countries.ToListAsync();
                        return View(existingBiodata);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(behalfOf))
                    {
                        existingBiodata.BehalfOf = behalfOf.ToLower();

                        if (existingBiodata.BehalfOf == "daughter" || existingBiodata.BehalfOf == "sister")
                        {
                            if (biodata.Gender?.ToLower() != "female")
                            {
                                ModelState.AddModelError(string.Empty, "Only female profiles are allowed for Daughter or Sister.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(existingBiodata);
                            }
                        }
                        else if (existingBiodata.BehalfOf == "friend")
                        {
                            if (biodata.Gender?.ToLower() == "male" && !hasPremium)
                            {
                                ModelState.AddModelError(string.Empty, "You cannot edit a male profile for a Friend without a Premium subscription.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(existingBiodata);
                            }
                        }
                        else if (existingBiodata.BehalfOf == "son" || existingBiodata.BehalfOf == "brother")
                        {
                            if (biodata.Gender?.ToLower() != "male")
                            {
                                ModelState.AddModelError(string.Empty,
                                    $"Only male profiles are allowed for {existingBiodata.BehalfOf}.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(existingBiodata);
                            }
                            if (!hasPremium)
                            {
                                ModelState.AddModelError(string.Empty, $"You cannot edit a profile for {existingBiodata.BehalfOf} without a Premium subscription.");
                                ViewBag.Countries = await _context.Countries.ToListAsync();
                                return View(existingBiodata);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "You can only edit profiles on behalf of Daughter, Sister, Friend, Son, or Brother.");
                            ViewBag.Countries = await _context.Countries.ToListAsync();
                            return View(existingBiodata);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "BehalfOf is required when creating for others.");
                        ViewBag.Countries = await _context.Countries.ToListAsync();
                        return View(existingBiodata);
                    }
                }
            }

            existingBiodata.IsForSelf = isForSelfBool;
            if (isForSelfBool)
            {
                existingBiodata.BehalfOf = "self";
            }
            else
            {
                existingBiodata.BehalfOf = behalfOf;
            }
            existingBiodata.FirstName = biodata.FirstName;
            existingBiodata.LastName = biodata.LastName;
            existingBiodata.DOB = biodata.DOB;
            existingBiodata.TimeOfBirth = biodata.TimeOfBirth;
            existingBiodata.Gender = biodata.Gender;
            existingBiodata.PlaceOfBirth = biodata.PlaceOfBirth;
            existingBiodata.Height = biodata.Height;
            existingBiodata.Weight = biodata.Weight;
            existingBiodata.PhysicalStatus = biodata.PhysicalStatus;
            existingBiodata.CountryId = biodata.CountryId;
            existingBiodata.StateId = biodata.StateId;
            existingBiodata.CityId = biodata.CityId;
            var selectedEducation = form["Education"];
            existingBiodata.Education = string.Join(",", selectedEducation);
            existingBiodata.Cast = biodata.Cast;
            existingBiodata.Subcaste = biodata.Subcaste;
            existingBiodata.Income = biodata.Income;
            existingBiodata.MotherTongue = biodata.MotherTongue;
            existingBiodata.MaritalStatus = biodata.MaritalStatus;
            existingBiodata.FatherName = biodata.FatherName;
            existingBiodata.FathersStatus = biodata.FathersStatus;
            existingBiodata.MotherName = biodata.MotherName;
            existingBiodata.MothersStatus = biodata.MothersStatus;
            existingBiodata.NumberOfBrothers = biodata.NumberOfBrothers;
            existingBiodata.NumberOfSisters = biodata.NumberOfSisters;
            //existingBiodata.MobileNumber = biodata.MobileNumber;
            existingBiodata.Email = biodata.Email;
            //existingBiodata.Address = biodata.Address;
            existingBiodata.HouseType = biodata.HouseType;
            existingBiodata.PartnerMaritalStatus = biodata.PartnerMaritalStatus;
            existingBiodata.PartnerEducation = biodata.PartnerEducation;
            existingBiodata.PartnerCast = biodata.PartnerCast;
            existingBiodata.PartnerCountry = biodata.PartnerCountry;
            existingBiodata.AboutMyPartner = biodata.AboutMyPartner;
            existingBiodata.VipBiodata = biodata.VipBiodata;

            var existingImagesCount = existingBiodata.Images.Count;
            if (images != null && images.Count > 0 && (existingImagesCount + images.Count) <= 5)
            {
                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/databio");
                        if (!Directory.Exists(uploads))
                        {
                            Directory.CreateDirectory(uploads);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var biodataImage = new BiodataImage
                        {
                            BiodataId = biodata.BiodataId,
                            ImageUrl = Path.Combine("images/databio", fileName),
                            IsDefault = false
                        };

                        _context.BiodataImages.Add(biodataImage);
                    }
                }
            }
            else if (existingImagesCount + (images?.Count ?? 0) > 5)
            {
                ModelState.AddModelError("Images", "You cannot upload more than 5 images.");
                return View(biodata);
            }
            foreach (var img in existingBiodata.Images)
            {
                img.IsDefault = false;
            }
            if (defaultImageIndex.HasValue)
            {
                int index = defaultImageIndex.Value;
                var imagesList = existingBiodata.Images.ToList();

                if (index >= 0 && index < imagesList.Count)
                {
                    imagesList[index].IsDefault = true;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("dashboard", "home");
        }

        [Authorize]
        [HttpPost]
        public IActionResult ShowInterest(int biodataId, int viewedBiodataId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }
            var loggedInUserId = int.Parse(userIdClaim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == loggedInUserId);
            if (user.Role != "Admin")
            {
                var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == loggedInUserId && s.IsActive && s.EndDate > DateTime.Now);

                if (subscription == null || (subscription.PlanType != PlanType.Premium))
                {
                    return Json(new { success = false, redirectUrl = Url.Action("SubscriptionDetails", "Payment"), message = "Please subscribe to show interest." });
                }
            }

            var viewedBiodata = _context.Biodata.FirstOrDefault(b => b.BiodataId == viewedBiodataId);
            if (viewedBiodata == null)
            {
                return Json(new { success = false, message = "Viewed profile not found." });
            }
            var interestedUserId = viewedBiodata.UserId;

            var existingInterest = _context.InterestedProfiles
                .FirstOrDefault(ip => ip.InterestedBiodataId == viewedBiodataId && ip.BiodataId == biodataId);

            if (existingInterest == null)
            {
                var biodata = _context.Biodata.FirstOrDefault(b => b.BiodataId == biodataId);
                if (biodata != null)
                {
                    var interest = new InterestedProfile
                    {
                        InterestedUserId = interestedUserId,
                        BiodataId = biodataId,
                        InterestedBiodataId = viewedBiodataId,
                        InterestedAt = DateTime.Now
                    };

                    _context.InterestedProfiles.Add(interest);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Interest shown successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Your profile not found." });
                }
            }

            return Json(new { success = false, message = "You have already shown interest in this profile." });
        }

        public async Task<IActionResult> GeneratePdf(int id)
        {
            var biodata = _context.Biodatas
                .Include(b => b.User)
                .Include(b => b.Images.OrderByDescending(i => i.IsDefault))
                .Include(b => b.Country)
                .Include(b => b.State)
                .Include(b => b.City)
                .FirstOrDefault(b => b.BiodataId == id);

            if (biodata == null)
            {
                return NotFound("Biodata not found.");
            }
            var age = DateTime.Now.Year - biodata.DOB.Year;
            if (biodata.DOB.Date > DateTime.Now.AddYears(-age)) age--;
            string profilePictureUrl = biodata.Images?.FirstOrDefault()?.ImageUrl ?? "default-profile.png";
            profilePictureUrl = profilePictureUrl.Replace("\\", "/");
            string baseUrl = $"http://192.168.1.79/";
            profilePictureUrl = $"{baseUrl}{profilePictureUrl}";
            string backgroundImageUrl = $"{baseUrl}assetsuser/images/mobile-app/pdfbiodata.png";
            string logoImageUrl = $"{baseUrl}assetsuser/images/mobile-app/pdfbiodatalogo.png";
            string ganeshImageUrl = $"{baseUrl}assetsuser/images/mobile-app/ganesh.png";
            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "template", "Biodata.html");
            string htmlContent = System.IO.File.ReadAllText(templatePath);
            htmlContent = htmlContent
                .Replace("{{profileid}}", $"{biodata.ProfileId}")
                .Replace("{{name}}", $"{biodata.FirstName} {biodata.LastName}")
                .Replace("{{dob}}", biodata.DOB.ToString("dd-MM-yyyy"))
                .Replace("{{age}}", age.ToString())
                .Replace("{{height}}", biodata.Height)
                .Replace("{{placeOfBirth}}", biodata.PlaceOfBirth ?? "N/A")
                .Replace("{{education}}", biodata.Education ?? "N/A")
                .Replace("{{cast}}", biodata.Cast ?? "N/A")
                .Replace("{{MotherTongue}}", biodata.MotherTongue ?? "N/A")
                .Replace("{{income}}", biodata.Income ?? "N/A")
                .Replace("{{maritalStatus}}", biodata.MaritalStatus ?? "N/A")
                .Replace("{{fatherName}}", biodata.FatherName ?? "N/A")
                .Replace("{{fathersstatus}}", biodata.FathersStatus ?? "N/A")
                .Replace("{{motherName}}", biodata.MotherName ?? "N/A")
                .Replace("{{mothersstatus}}", biodata.MothersStatus ?? "N/A")
                .Replace("{{siblings}}", $"{biodata.NumberOfBrothers} Brothers, {biodata.NumberOfSisters} Sisters")
                //.Replace("{{contactNumber}}", biodata.MobileNumber)
                .Replace("{{email}}", biodata.Email ?? "N/A")
                .Replace("{{country}}", biodata.Country.CountryName ?? "N/A")
                .Replace("{{state}}", biodata.State.StateName ?? "N/A")
                .Replace("{{city}}", biodata.City.CityName ?? "N/A")
                //.Replace("{{address}}", biodata.Address ?? "N/A")
                .Replace("{{houseType}}", biodata.HouseType ?? "N/A")
                .Replace("{{profilePicture}}", profilePictureUrl)
                .Replace("{{backgroundImage}}", backgroundImageUrl)
                .Replace("{{logoImage}}", logoImageUrl)
                .Replace("{{ganeshImage}}", ganeshImageUrl);

            string chromiumPath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromiumPath
            });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0mm",
                    Bottom = "0mm",
                    Left = "0mm",
                    Right = "0mm"
                }
            });
            await browser.CloseAsync();
            return File(pdfBytes, "application/pdf", $"{biodata.FirstName}_Biodata.pdf");
        }

    }
}