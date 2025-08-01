using Microsoft.AspNetCore.Mvc;

namespace suvarnyug.Controllers
{
    public class SharedController : Controller
    {
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
