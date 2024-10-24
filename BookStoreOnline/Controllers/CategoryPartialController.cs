using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class CategoryPartialController : Controller
    {
        NhaSachEntities3 db = new NhaSachEntities3();
        // GET: CategoryPartial
        public ActionResult GetPartialCategoryNav()
        {
            ViewBag.CateList = db.LOAIs.ToList();
            return PartialView();
        }

        public ActionResult GetCategorySelection()
        {
            ViewBag.CateList = db.LOAIs.ToList();
            return PartialView();
        }
    }
}