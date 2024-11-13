using System; 
using System.IdentityModel.Tokens.Jwt; 
using System.Linq; 
using System.Security.Claims; 
using System.Text; 
using System.Web.Mvc; 
using Microsoft.IdentityModel.Tokens;
using BookStoreOnline.Models;
using System.Security.Cryptography;
using System.Web;

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
                new Claim(ClaimTypes.Name, user.Ten), 
                new Claim(ClaimTypes.Email, user.Email), 
                new Claim("TrangThai", user.TrangThai.ToString()), 
                new Claim("NgayTao", user.NgayTao?.ToString("yyyy-MM-dd HH:mm:ss")) 

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
    private void SetAccessTokenCookie(string accessToken)
    {

        var cookie = new HttpCookie("accessToken", accessToken)
        {
            HttpOnly = true, 
            Secure = Request.IsSecureConnection, 
            Expires = DateTime.UtcNow.AddHours(1) 
        };
        Response.Cookies.Add(cookie);
    }
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookie = new HttpCookie("refreshToken", refreshToken)
        {
            HttpOnly = true, // Bảo mật: Cookie chỉ có thể truy cập từ server
            Secure = Request.IsSecureConnection, // Chỉ gửi cookie qua HTTPS nếu có
            Expires = DateTime.UtcNow.AddDays(7) // Thời hạn của refresh token
        };
        Response.Cookies.Add(cookie);
    }

    private string GetRefreshTokenFromCookie()
    {
        var cookie = Request.Cookies["refreshToken"];
        return cookie != null ? cookie.Value : null;
    }

    private void RemoveRefreshTokenCookie()
    {
        if (Request.Cookies["refreshToken"] != null)
        {
            var cookie = new HttpCookie("refreshToken")
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            Response.Cookies.Add(cookie);
        }
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
            if (cus.MatKhau.Length < 6)
            {
                ViewBag.ThongBaoMK = "Mật khẩu phải có ít nhất 6 ký tự";
                return View();
            }

            if (cus.MatKhau == rePass)
            {
                cus.TrangThai = true;
                cus.NgayTao = DateTime.Now;
                cus.MatKhau = HashPassword(cus.MatKhau);

                db.KHACHHANGs.Add(cus);
                db.SaveChanges();
                ViewBag.ThongBao = "Đăng ký thành công";

                // Tạo token
                var accessToken = GenerateAccessToken(cus);
                var refreshToken = GenerateRefreshToken();

                // Lưu cả access_token và refresh_token vào cơ sở dữ liệu
                cus.RefreshToken = refreshToken;
                cus.AccessToken = accessToken;
                cus.TokenExpiration = DateTime.UtcNow.AddDays(7); // set refresh_token for 7 days
                db.SaveChanges();

                // Lưu access token vào cookie
                SetAccessTokenCookie(accessToken);

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

                // Tạo token mới
                var accessToken = GenerateAccessToken(account);
                var refreshToken = GenerateRefreshToken();

                // Lưu access_token vào cơ sở dữ liệu
                account.AccessToken = accessToken;
                account.TokenExpiration = DateTime.UtcNow.AddDays(7);
                db.SaveChanges();

                // Lưu access token vào cookie
                SetAccessTokenCookie(accessToken);

                // Lưu refresh token vào cookie
                SetRefreshTokenCookie(refreshToken);

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
    private string GetAccessTokenFromCookie()
    {
        var cookie = Request.Cookies["accessToken"];
        return cookie != null ? cookie.Value : null;
    }
    [HttpPost]
    public ActionResult RefreshToken()
    {
        var refreshToken = GetRefreshTokenFromCookie();

        if (string.IsNullOrEmpty(refreshToken))
        {
            return new HttpStatusCodeResult(401, "Refresh Token không hợp lệ hoặc đã hết hạn");
        }

        var user = db.KHACHHANGs.FirstOrDefault(u => u.RefreshToken == refreshToken && u.TokenExpiration > DateTime.UtcNow);

        if (user == null)
        {
            return new HttpStatusCodeResult(401, "Refresh Token không hợp lệ hoặc đã hết hạn");
        }

        // Tạo access token mới
        var newAccessToken = GenerateAccessToken(user);
        user.AccessToken = newAccessToken;
        db.SaveChanges();

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

        RemoveRefreshTokenCookie();

        return RedirectToAction("Login", "User"); 
    } 
} 
