using System;
using System.Linq;
using System.Web.Mvc;
using BookStoreOnline.Models; // Đảm bảo bạn đã thêm namespace cho mô hình dữ liệu của bạn

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class Home_PageController : Controller
    {
        private NhaSachEntities2 db = new NhaSachEntities2();

        // GET: Admin/HomePage
        public ActionResult Index()
        {
            // Đếm tổng số khách hàng
            var soLuongKhachHang = db.KHACHHANGs.Count();


            // Gửi thông tin số lượng khách hàng tới view
            ViewBag.SoLuongKhachHang = soLuongKhachHang;
            ViewBag.TongSanPham = db.SANPHAMs.Count(); // Tổng số sản phẩm
            ViewBag.TongDonHang = db.DONHANGs.Count(); // Tổng số đơn hàng
            ViewBag.TongLoai = db.LOAIs.Count();//
                                                // Lấy danh sách đơn hàng từ cơ sở dữ liệu
            var donHangs = db.DONHANGs
                .OrderBy(d => d.MaDonHang) // Sắp xếp theo ID đơn hàng
                .Take(5) // Lấy 5 đơn hàng đầu tiên
                .ToList();

            // Gửi danh sách đơn hàng tới View
            ViewBag.DonHangs = donHangs;

            return View();
        }
    }
}