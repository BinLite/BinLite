using BinLite.Models;

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

namespace BinLite.Controllers
{
    public class EntityController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public EntityController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => Redirect("/");

        public IActionResult List(string entity)
        {
            ViewData["entity"] = entity;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}