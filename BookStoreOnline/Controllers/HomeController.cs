using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class HomeController : Controller
    {
        NhaSachEntities2 db = new NhaSachEntities2();
        public ActionResult Index()
        {
            var book = db.SANPHAMs.ToList().Take(8);
            return View(book);
        }
    }
}