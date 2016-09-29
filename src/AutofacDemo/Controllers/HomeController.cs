using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutofacDemo.Codes;
using Microsoft.AspNetCore.Mvc;

namespace AutofacDemo.Controllers
{
    public class HomeController : Controller
    {
        private IUserManager _User;
        public HomeController(IUserManager user)
        {
            _User = user;
        }

        public IActionResult Index()
        {
            _User.Register("hello");

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
