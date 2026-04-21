using Flights.Data;
using Flights.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Flights.Controllers
{
    public class HomeController : Controller
    {

        private readonly FlightsContext _context;

        private readonly ILogger<HomeController> _logger;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(FlightsContext context, ILogger<HomeController> logger, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Flights()
        {
            return View();
        }

        public ActionResult FlightsCreate()
        {
            return View();
        }

        public async Task<ActionResult> FlightsEdit(int id)
        {
            var flight = await _context.Flights.SingleOrDefaultAsync(m => m.FlightId == id);
            return View(flight);
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
