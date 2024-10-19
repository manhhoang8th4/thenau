using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        private NhaSachEntities2 db = new NhaSachEntities2();

        // GET: Admin/LOAIs.
        public ActionResult Index()
        {
            return View(db.LOAIs.ToList());
        }

        // GET: Admin/LOAIs.Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAI loai = db.LOAIs.Find(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // GET: Admin/LOAIs.Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/LOAIs.Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLoai,TenLoai")] LOAI loai)
        {
            if (ModelState.IsValid)
            {
                db.LOAIs.Add(loai);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(loai);
        }

        // GET: Admin/LOAIs.Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAI loai = db.LOAIs.Find(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // POST: Admin/LOAIs.Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLoai, TenLoai")] LOAI loai)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loai).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(loai);
        }

        // GET: Admin/LOAIs.Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAI loai = db.LOAIs.Find(id);
            if (loai == null)
            {
                return HttpNotFound();
            }
            return View(loai);
        }

        // POST: Admin/LOAIs.Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LOAI loai = db.LOAIs.Find(id);
            db.LOAIs.Remove(loai);
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
