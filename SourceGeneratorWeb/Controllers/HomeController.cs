using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SourceGeneratorWeb.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SourceGeneratorWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExampleService exampleService;

        public HomeController(ILogger<HomeController> logger, ExampleService exampleService)
        {
            _logger = logger;
            this.exampleService = exampleService;
        }

        public IActionResult Index()
        {
            return View((object)exampleService.GetValue());
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
