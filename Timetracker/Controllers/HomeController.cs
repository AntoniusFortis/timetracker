using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.Models;

namespace Timetracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly TimetrackerContext _timetrackerContext;

        public HomeController(TimetrackerContext timetrackerContext)
        {
            _timetrackerContext = timetrackerContext;
        }

        public IActionResult Index()
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
