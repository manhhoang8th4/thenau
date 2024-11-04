using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class CategoryController : Controller
    {
        NhaSachEntities3 db = new NhaSachEntities3();

        // GET: Category
        public ActionResult Index(int id)
        {
            ViewBag.CategoryName = db.LOAIs.FirstOrDefault(n => n.Maloai == id)?.Tenloai; // Use null-conditional operator
            return View(db.SANPHAMs.Where(book => book.MaLoai == id).ToList());
        }

        public ActionResult GetAllBook()
        {
            return View(db.SANPHAMs.ToList());
        }

        public ActionResult Search(string inputString)
        {
            ViewBag.TextSearch = inputString; // Corrected variable name
            var result = db.SANPHAMs
                .Where(s => s.TenSanPham.Contains(inputString) ||
                            s.TacGia.Contains(inputString) ||
                            s.LOAI.Tenloai.Contains(inputString)) // Search by category name
                .ToList();

            return View("Search", result); // Render the Search view with the result
        }
    }
}
