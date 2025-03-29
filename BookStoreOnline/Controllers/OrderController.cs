using System.Data.Entity;
using System.Web.Mvc;
using BookStoreOnline.Models;
using BookStoreOnline.Areas.Admin.Constants;
using System.Linq;
using System.Net;

namespace BookStoreOnline.Controllers
{
    public class OrderController : Controller
    {
        private NhaSachEntities3 db = new NhaSachEntities3();

        // GET: Order
        private KHACHHANG GetAuthenticatedUser()
        {
            var cookie = Request.Cookies["AccessToken"];
            if (cookie == null)
            {
                return null;
            }

            string accessToken = cookie.Value;
            return db.KHACHHANGs.FirstOrDefault(k => k.AccessToken == accessToken);
        }

        // GET: Order
        public ActionResult Index()
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var orders = db.DONHANGs.Where(o => o.ID == user.MaKH).ToList();

            foreach (var order in orders)
            {
                if (order.TrangThai == (int)Constants.StatusOrder.Received && order.TrangThaiThanhToan != (int)Constants.StatusPayment.Paid)
                {
                    order.TrangThaiThanhToan = (int)Constants.StatusPayment.Paid;
                    db.Entry(order).State = EntityState.Modified;
                }
            }

            db.SaveChanges(); // Lưu cập nhật vào database

            return View(orders);
        }
        [HttpPost]  
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string reason)
        {
            var order = db.DONHANGs.Find(id);

            if (order != null && order.TrangThai == (int)Constants.StatusOrder.NoInform) // Chỉ cập nhật trạng thái cho đơn hàng chưa xác nhận
            {
                order.TrangThai = (int)Constants.StatusOrder.Canceled;

                // Nếu đơn hàng đã được thanh toán, cập nhật trạng thái thanh toán thành Đã Hoàn Tiền
                if (order.TrangThaiThanhToan == (int)Constants.StatusPayment.Paid)
                {
                    order.TrangThaiThanhToan = (int)Constants.StatusPayment.Refund;
                }

                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công và đã hoàn tiền";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn lý do hủy đơn hoặc đơn hàng không hợp lệ.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var order = db.DONHANGs
                .Include(o => o.CHITIETDONHANGs.Select(d => d.SANPHAM)) // Nạp thông tin sản phẩm
                .FirstOrDefault(o => o.MaDonHang == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
    }
}
