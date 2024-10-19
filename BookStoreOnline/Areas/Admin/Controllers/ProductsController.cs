using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private NhaSachEntities2 db = new NhaSachEntities2();

        public ActionResult Index(string searchString)
        {
            // Khai báo sanPham dưới dạng IQueryable
            IQueryable<SANPHAM> sanPham = db.SANPHAMs.OrderByDescending(p => p.MaSanPham);

            if (!String.IsNullOrEmpty(searchString))
            {
                sanPham = sanPham.Where(s => s.TenSanPham.Contains(searchString));
            }

            return View(sanPham.ToList()); // Chuyển đổi thành List khi truyền vào View

        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sanPham = db.SANPHAMs.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.LoaiSP = new SelectList(db.LOAIs, "MaLoai", "TenLoai");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaSanPham,TenSanPham,Gia,MoTa,TacGia,Anh,MaLoai,SoLuong")] SANPHAM sanPham, HttpPostedFileBase imageBook)
        {
            if (ModelState.IsValid)
            {
                if (imageBook != null)
                {
                    var fileName = Path.GetFileName(imageBook.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    sanPham.Anh = fileName;
                    imageBook.SaveAs(path);
                }

                db.SANPHAMs.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Khi có lỗi, nạp lại danh sách thể loại
            ViewBag.LoaiSP = new SelectList(db.LOAIs, "MaLoai", "TenLoai", sanPham.MaLoai);
            return View(sanPham);
        }
        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SANPHAM sanPham = db.SANPHAMs.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Nạp danh sách thể loại vào ViewBag
            ViewBag.LoaiSP = new SelectList(db.LOAIs, "MaLoai", "TenLoai", sanPham.MaLoai);

            return View(sanPham);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaSanPham,TenSanPham,Gia,MoTa,TacGia,Anh,MaLoai,SoLuong")] SANPHAM sanPham, HttpPostedFileBase imageBook)
        {
            if (ModelState.IsValid)
            {
                if (imageBook != null)
                {
                    var fileName = Path.GetFileName(imageBook.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    sanPham.Anh = fileName;
                    imageBook.SaveAs(path);
                }

                db.Entry(sanPham).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.LOAIs, "CategoryID", "CategoryName", sanPham.MaLoai);
            return View(sanPham);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sanPham = db.SANPHAMs.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SANPHAM product = db.SANPHAMs.Find(id);
            db.SANPHAMs.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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
