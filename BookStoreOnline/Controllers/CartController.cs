using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PayPal.Api;
using BookStoreOnline.Models;
using System.Web.Util;
using System.Threading.Tasks;

namespace BookStoreOnline.Controllers
{
    public class CartController : Controller
    {
        private readonly NhaSachEntities3 db = new NhaSachEntities3();

        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        // Get the current cart from session or create a new one if it doesn't exist
        private List<CartItem> GetCart()
        {
            if (Session["GioHang"] is List<CartItem> cart)
            {
                return cart;
            }


            cart = new List<CartItem>();
            Session["GioHang"] = cart;
            return cart;
        }

        // Save the cart to the session
        private void SaveCart(List<CartItem> cart)
        {
            Session["GioHang"] = cart;
        }

        // Add product to cart
        [HttpPost]
        public ActionResult AddToCart(FormCollection product)
        {
            var cart = GetCart();

            if (!int.TryParse(product["ProductID"], out var productId) ||
                !int.TryParse(product["Quantity"], out var quantity))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid input");
            }

            var productInDb = db.SANPHAMs.Find(productId);
            if (productInDb == null)
            {
                return HttpNotFound("Product not found");
            }

            var cartItem = cart.FirstOrDefault(p => p.ProductID == productId);
            if (cartItem == null)
            {
                if (quantity > productInDb.SoLuong)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quá số lượng tồn trong kho");
                }

                cartItem = new CartItem(productId)
                {
                    Number = quantity
                };
                cart.Add(cartItem);
            }
            else
            {
                if (cartItem.Number + quantity > productInDb.SoLuong)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quá số lượng tồn trong kho.");
                }

                cartItem.Number += quantity;
            }
            SaveCart(cart);
            return RedirectToAction("GetCartInfo");
        }

        // Add a single product to the cart
        public ActionResult AddSingleProduct(int id)
        {
            var cart = GetCart();
            const int quantity = 1;

            var productInDb = db.SANPHAMs.Find(id);
            if (productInDb == null)
            {
                return HttpNotFound("Product not found");
            }

            var cartItem = cart.FirstOrDefault(p => p.ProductID == id);
            if (cartItem == null)
            {
                if (quantity > productInDb.SoLuong)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quá số lượng tồn trong kho.");
                }

                cartItem = new CartItem(id)
                {
                    Number = quantity
                };
                cart.Add(cartItem);
            }
            else
            {
                if (cartItem.Number + quantity > productInDb.SoLuong)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "                .");
                }

                cartItem.Number += quantity;
            }
            SaveCart(cart);
            return RedirectToAction("GetCartInfo");
        }

        // Remove a product from the cart
        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(p => p.ProductID == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
            return RedirectToAction("GetCartInfo");
        }

        // Get total number of items in the cart
        private int GetTotalNumber()
        {
            var cart = GetCart();
            return cart.Sum(sp => sp.Number);
        }

        // Get total price of items in the cart
        private decimal GetTotalPrice()
        {
            var cart = GetCart();
            return cart.Sum(sp => sp.FinalPrice());
        }

        // Display cart information
        public ActionResult GetCartInfo()
        {
            var cart = GetCart();

            if (cart == null || !cart.Any())
            {
                return RedirectToAction("NullCart");
            }

            ViewBag.TotalNumber = GetTotalNumber();
            ViewBag.TotalPrice = GetTotalPrice();
            return View(cart);
        }

        // Update the quantity of a product in the cart
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            // Kiểm tra nếu số lượng không hợp lệ
            if (quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid quantity." }, JsonRequestBehavior.AllowGet);
            }

            // Tìm kiếm sản phẩm trong cơ sở dữ liệu
            var product = db.SANPHAMs.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." }, JsonRequestBehavior.AllowGet);
            }

            // Kiểm tra nếu số lượng yêu cầu lớn hơn số lượng tồn kho
            if (quantity > product.SoLuong)
            {
                return Json(new { success = false, message = "Quá số lượng tồn trong kho", validQuantity = 1 }, JsonRequestBehavior.AllowGet);
            }

            // Lấy giỏ hàng từ session
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(item => item.ProductID == productId);

            if (cartItem != null)
            {
                // Cập nhật số lượng của sản phẩm trong giỏ hàng
                cartItem.Number = quantity;
                SaveCart(cart); // Lưu giỏ hàng vào session hoặc cơ sở dữ liệu

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Product not found in the cart." }, JsonRequestBehavior.AllowGet);
            }
        }

        // Partial view for cart summary
        public ActionResult CartPartial()
        {
            ViewBag.TotalNumber = GetTotalNumber();
            return PartialView();
        }

        // View for empty cart
        public ActionResult NullCart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CheckStock(int productId, int quantity)
        {
            var product = db.SANPHAMs.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            if (quantity > product.SoLuong)
            {
                return Json(new { success = false, message = "Quá số lượng tồn trong kho" });
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult InsertOrder(string address)
        {
            var cartItems = GetCart();
            if (cartItems == null || !cartItems.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Empty cart.");
            }

            var customer = Session["TaiKhoan"] as KHACHHANG;
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Not logged in.");
            }

            var discountAmount = Session["DiscountAmount"] as decimal? ?? 0;
            var finalPrice = Session["FinalPrice"] as decimal? ?? GetTotalPrice();
            var roundedFinalPrice = (int)Math.Round(finalPrice);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new DONHANG
                    {
                        ID = customer.MaKH,
                        NgayDat = DateTime.Now,
                        DiaChi = address,
                        TrangThai = 0, // Not confirmed
                        TongTien = roundedFinalPrice
                    };
              

                    db.DONHANGs.Add(order);
                    db.SaveChanges();

                    foreach (var item in cartItems)
                    {
                        var product = db.SANPHAMs.Find(item.ProductID);
                        if (product == null)
                        {
                            return HttpNotFound("Product not found.");
                        }

                        if (item.Number > product.SoLuong)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quá số lượng tồn trong kho.");
                        }

                        var orderDetail = new CHITIETDONHANG
                        {
                            MaDonHang = order.MaDonHang,
                            MaSanPham = item.ProductID,
                            SoLuong = item.Number
                        };
                        db.CHITIETDONHANGs.Add(orderDetail);

                        product.SoLuong -= item.Number;
                        product.SoLuongBan += item.Number;
                        db.Entry(product).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    // Do not clear cart until PayPal payment is confirmed
                    Session["GioHang"] = null;
                    Session["DiscountAmount"] = null;
                    Session["FinalPrice"] = null;

                    transaction.Commit();

                    return RedirectToAction("momo", "Cart", new { id = order.MaDonHang });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Order processing error.");
                }
            }
        }
        public ActionResult InsertOrder1(string address)
        {
            var cartItems = GetCart();
            if (cartItems == null || !cartItems.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Empty cart.");
            }

            var customer = Session["TaiKhoan"] as KHACHHANG;
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Not logged in.");
            }

            var discountAmount = Session["DiscountAmount"] as decimal? ?? 0;
            var finalPrice = Session["FinalPrice"] as decimal? ?? GetTotalPrice();
            var roundedFinalPrice = (int)Math.Round(finalPrice);

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var order = new DONHANG
                    {
                        ID = customer.MaKH,
                        NgayDat = DateTime.Now,
                        DiaChi = address,
                        TrangThai = 0, // Not confirmed
                        TongTien = roundedFinalPrice
                    };

                    db.DONHANGs.Add(order);
                    db.SaveChanges();

                    foreach (var item in cartItems)
                    {
                        var product = db.SANPHAMs.Find(item.ProductID);
                        if (product == null)
                        {
                            return HttpNotFound("Product not found.");
                        }

                        if (item.Number > product.SoLuong)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quá số lượng tồn trong kho.");
                        }

                        var orderDetail = new CHITIETDONHANG
                        {
                            MaDonHang = order.MaDonHang,
                            MaSanPham = item.ProductID,
                            SoLuong = item.Number
                        };
                        db.CHITIETDONHANGs.Add(orderDetail);

                        product.SoLuong -= item.Number;
                        product.SoLuongBan += item.Number;
                        db.Entry(product).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    // Do not clear cart until PayPal payment is confirmed
                    Session["GioHang"] = null;
                    Session["DiscountAmount"] = null;
                    Session["FinalPrice"] = null;

                    transaction.Commit();

                    return RedirectToAction("Index","Order");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Order processing error.");
                }
            }
        }

        [HttpPost]
        public JsonResult ApplyDiscount(string discountCode)
        {
            var discount = db.KHUYENMAIs.FirstOrDefault(d => d.MaKM == discountCode && d.KichHoat == true);
            decimal discountAmount = 0;
            decimal totalPrice = GetTotalPrice(); // Get the current total price before discount

            if (discount != null)
            {
                if (totalPrice >= discount.SoTienMuaHangToiThieu)
                {
                    discountAmount = discount.SoTienKM;
                    totalPrice -= discountAmount; // Apply discount
                    Session["DiscountAmount"] = discountAmount;
                    Session["FinalPrice"] = totalPrice;
                }
                else
                {
                    return Json(new { success = false, message = "Không đạt yêu cầu tối thiểu để áp dụng mã khuyến mãi." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn" });
            }

            return Json(new { success = true, discountAmount = discountAmount, finalPrice = totalPrice });
        }
        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {

                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/cart/PaymentWithPayPal?";

                    var guid = Convert.ToString((new Random()).Next(100000));

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            return View("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
      
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            /* List<CartItem> listSanPham = Session["GioHang"] as List<CartItem>;
             //create itemlist and add item objects to it  
             var itemList = new ItemList()
             {
                 items = new List<Item>()
             };
             //Adding Item Details like name, currency, price etc  

             foreach (var item in listSanPham)
             {
                 itemList.items.Add(new Item()
                 {
                     name = item.TenSanPham,
                     currency = "USD",
                     price = item.Gia.ToString(),
                     quantity = item.SoLuong.ToString(),
                     sku = item.MaSanPham.ToString(),
                 });
             }*/

            //Testtttttttttttttttttttttttttttttttttttttttttttttttttt

            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "Giá sản phẩm",
                currency = "USD",
                price = "0",
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = "22.4"
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = "22.4", // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
        public async Task<ActionResult> momo(int id)
        {
           
            var checkid = db.DONHANGs.Where(s => s.MaDonHang == id).FirstOrDefault();
            var tongtien = checkid.TongTien;
            var paymentService = new PaymentService();
            string orderInfo = $"ma kac hasnh - {id}";
            string redirectUrl = Url.Action("callback", "cart", new { id = id }, Request.Url.Scheme);
            string callbackUrl = Url.Action("PremiumFailure", "truyen", null, Request.Url.Scheme);
            string paymentUrl = await paymentService.CreateMoMoPaymentAsync(tongtien, orderInfo, redirectUrl, callbackUrl);

            if (string.IsNullOrEmpty(paymentUrl))
            {
                throw new Exception("Failed to create MoMo payment URL");
            }
            return Redirect(paymentUrl);
        }
       public ActionResult callback(int id)
       {
            var checkid = db.DONHANGs.Where(s=> s.MaDonHang == id).FirstOrDefault();
            if(checkid == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Index","Order");
       }
    }
}