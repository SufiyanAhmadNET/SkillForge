using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Models;
using SkillForge.Interfaces;

namespace SkillForge.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseQueryService _courseQueryService;

        public HomeController(ILogger<HomeController> logger, ICourseQueryService courseQueryService)
        {
            _logger = logger;
            _courseQueryService = courseQueryService;
        }

        public IActionResult Index()
        {
            var vm = _courseQueryService.GetPublishedCoursePage(0);
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
