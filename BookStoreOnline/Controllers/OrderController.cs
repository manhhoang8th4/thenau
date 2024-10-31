using System.Data.Entity;
using System.Web.Mvc;
using BookStoreOnline.Models;
using BookStoreOnline.Areas.Admin.Constants; // Thêm dòng này nếu chưa có
using System.Linq;

namespace BookStoreOnline.Controllers
{
    public class OrderController : Controller
    {
        private NhaSachEntities3 db = new NhaSachEntities3();

        // GET: Order
        public ActionResult Index()
        {
            var orders = db.DONHANGs.ToList();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string reason) 
        {
            var order = db.DONHANGs.Find(id);

            if (order != null && order.TrangThai == 0) // Chỉ cập nhật trạng thái cho đơn hàng chưa gửi
            {
                order.TrangThai = (int)Constants.StatusOrder.Canceled; 
                // order.ReasonForCancellation = reason; // Giả sử bạn có trường này trong model

                db.Entry(order).State = EntityState.Modified; 
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng chọn lý do hủy đơn";
            }

            return RedirectToAction("Index");
        }
    }
}
