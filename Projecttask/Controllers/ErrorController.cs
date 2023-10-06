using Microsoft.AspNetCore.Mvc;

namespace Projecttask.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the page you requested could not be found.";
                    return View("404");
                case 401:
                    ViewBag.ErrorMessage = "Sorry, you don't have permission.";
                    return View("404");
            }

            return View("Error");
        }
    }
}
