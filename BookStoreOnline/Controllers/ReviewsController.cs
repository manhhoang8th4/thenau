using System;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class ReviewsController : Controller
    {
        private NhaSachEntities3 db = new NhaSachEntities3();

        // GET: Reviews/Create
        public ActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,MaKH,MaSanPham,NoiDung,NgayTao,SoSao")] DANHGIA review)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem người dùng đã có đánh giá cho sản phẩm này chưa
                var lastReview = db.DANHGIAs
                                   .Where(r => r.MaKH == review.MaKH && r.MaSanPham == review.MaSanPham)
                                   .OrderByDescending(r => r.NgayTao)
                                   .FirstOrDefault();

                if (lastReview != null)
                {
                    // Kiểm tra thời gian giữa lần đánh giá gần nhất và lần đánh giá mới
                    var timeDifference = DateTime.Now - lastReview.NgayTao;
                    if (timeDifference.TotalMilliseconds < 5)
                    {
                        ViewBag.ErrorMessage = "Bạn cần chờ ít nhất 5 phút trước khi đánh giá lại sản phẩm này.";
                        ViewBag.ProductId = review.MaSanPham;
                        return View(review);  // Trả lại view và không cho phép đánh giá
                    }

                }
                
                // Nếu tất cả điều kiện đúng, lưu đánh giá mới
                review.NgayTao = DateTime.Now; // Lưu thời gian hiện tại
                db.DANHGIAs.Add(review);
                db.SaveChanges();
                return RedirectToAction("Details", "Products", new { id = review.MaSanPham });
            }

            // Nếu có lỗi form, trả lại View để người dùng sửa lỗi
            ViewBag.ProductId = review.MaSanPham;
            return View(review);
        }

        public ActionResult ProductReviews(int productId)
        {
            var reviews = db.DANHGIAs.Where(r => r.MaSanPham == productId).ToList();
            return PartialView("_ProductReviews", reviews);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
