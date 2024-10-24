using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web.Mvc;
using Microsoft.IdentityModel.Tokens;
using BookStoreOnline.Models;
using System.Security.Cryptography;

public class UserController : Controller
{
    private NhaSachEntities3 db = new NhaSachEntities3();

    private const string SecretKey = "your_new_very_long_secret_key_at_least_32_characters!";

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    private string GenerateAccessToken(KHACHHANG user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.MaKH.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(1), // set access_token for 1 hour
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [HttpGet]
    public ActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public ActionResult SignUp(KHACHHANG cus, string rePass)
    {
        if (ModelState.IsValid)
        {
            var checkEmail = db.KHACHHANGs.FirstOrDefault(c => c.Email == cus.Email);
            if (checkEmail != null)
            {
                ViewBag.ThongBaoEmail = "Đã có tài khoản đăng nhập bằng Email này";
                return View();
            }

            if (cus.MatKhau == rePass)
            {
                cus.TrangThai = true;
                cus.NgayTao = DateTime.Now;

                cus.MatKhau = HashPassword(cus.MatKhau);

                db.KHACHHANGs.Add(cus);
                db.SaveChanges();

                var accessToken = GenerateAccessToken(cus);
                var refreshToken = GenerateRefreshToken();

                cus.RefreshToken = refreshToken;
                cus.TokenExpiration = DateTime.UtcNow.AddDays(7); // set refresh_token for 7 days
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ThongBao = "Mật khẩu xác nhận không chính xác";
            }
        }
        return View();
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(KHACHHANG taikhoan)
    {
        if (ModelState.IsValid)
        {
            var taikhoanAdmin = db.NHANVIENs.FirstOrDefault(k => k.Email == taikhoan.Email && k.MatKhau == taikhoan.MatKhau);
            if (taikhoanAdmin != null)
            {
                if (!taikhoanAdmin.TrangThai)
                {
                    ViewBag.ThongBao = "Tài khoản đã bị khóa";
                    return View();
                }

                Session["TaiKhoan"] = taikhoanAdmin;
                return RedirectToAction("Index", "Home_Page", new { area = "Admin" });
            }

            var hashedPassword = HashPassword(taikhoan.MatKhau);
            var account = db.KHACHHANGs.FirstOrDefault(k => k.Email == taikhoan.Email && k.MatKhau == hashedPassword);

            if (account != null)
            {
                if (!account.TrangThai)
                {
                    ViewBag.ThongBao = "Tài khoản đã bị khóa";
                    return View();
                }

                var accessToken = GenerateAccessToken(account);
                var refreshToken = GenerateRefreshToken();

                account.RefreshToken = refreshToken;
                account.TokenExpiration = DateTime.UtcNow.AddDays(7);
                db.SaveChanges();

                Session["TaiKhoan"] = account;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ThongBao = "Email hoặc mật khẩu không chính xác";
            }
        }
        return View();
    }

    [HttpPost]
    public ActionResult RefreshToken(string refreshToken)
    {
        var user = db.KHACHHANGs.FirstOrDefault(u => u.RefreshToken == refreshToken && u.TokenExpiration > DateTime.UtcNow);

        if (user == null)
        {
            return new HttpStatusCodeResult(401, "Refresh Token không hợp lệ hoặc đã hết hạn");
        }

        var newAccessToken = GenerateAccessToken(user);
        return Json(new { accessToken = newAccessToken });
    }

    [HttpGet]
    public ActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
    {
        if (ModelState.IsValid)
        {
            var currentUser = Session["TaiKhoan"] as KHACHHANG;

            if (currentUser != null)
            {
                var user = db.KHACHHANGs.FirstOrDefault(u => u.MaKH == currentUser.MaKH);

                if (user != null)
                {
                    if (user.MatKhau == HashPassword(oldPassword))
                    {
                        if (newPassword == confirmPassword)
                        {
                            user.MatKhau = HashPassword(newPassword);
                            db.SaveChanges();
                            ViewBag.ThongBao = "Mật khẩu đã được thay đổi thành công!";
                        }
                        else
                        {
                            ViewBag.ThongBao = "Mật khẩu xác nhận không chính xác";
                        }
                    }
                    else
                    {
                        ViewBag.ThongBao = "Mật khẩu cũ không chính xác";
                    }
                }
                else
                {
                    ViewBag.ThongBao = "Người dùng không tồn tại";
                }
            }
            else
            {
                ViewBag.ThongBao = "Người dùng chưa đăng nhập";
            }
        }
        return View();
    }

    [HttpGet]
    public ActionResult UpdateInfo()
    {
        var currentUser = Session["TaiKhoan"] as KHACHHANG;

        if (currentUser != null)
        {
            var user = db.KHACHHANGs.FirstOrDefault(u => u.MaKH == currentUser.MaKH);
            if (user != null)
            {
                return View(user);
            }
        }
        return RedirectToAction("Login", "User");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateInfo(KHACHHANG updatedUser)
    {
        if (ModelState.IsValid)
        {
            var currentUser = Session["TaiKhoan"] as KHACHHANG;

            if (currentUser != null)
            {
                var user = db.KHACHHANGs.FirstOrDefault(u => u.MaKH == currentUser.MaKH);
                if (user != null)
                {
                    user.Ten = updatedUser.Ten;
                    user.Email = updatedUser.Email;
                    user.DiaChi = updatedUser.DiaChi;
                    user.SoDienThoai = updatedUser.SoDienThoai;

                    db.SaveChanges();
                    Session["TaiKhoan"] = user;

                    ViewBag.ThongBao = "Thông tin đã được cập nhật thành công!";
                }
                else
                {
                    ViewBag.ThongBao = "Người dùng không tồn tại";
                }
            }
            else
            {
                ViewBag.ThongBao = "Người dùng chưa đăng nhập";
            }
        }
        return View(updatedUser);
    }

    public ActionResult LogOut()
    {
        Session["TaiKhoan"] = null;
        Session["GioHang"] = null;

        return RedirectToAction("Login", "User");
    }
}
