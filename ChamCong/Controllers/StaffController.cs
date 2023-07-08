using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ChamCong.Models;
using PagedList;
//namespace sử dụng cho xuất file excel
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Drawing;
using System.Data.SqlClient;

namespace ChamCong.Controllers
{
    public class StaffController : Controller
    {
        EMPLOYEEEntities1 db = new EMPLOYEEEntities1();
        // GET: Staff

        //đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            string taikhoan = f["txtTaiKhoan"].ToString();
            string matkhau = f["txtMatKhau"].ToString();
            if (taikhoan == null || matkhau == null)
            {
                return RedirectToAction("login", "Staff");
            }
            //Email:tài khoản đăng nhập
            //Mật khẩu:sdt nhân viên
            tblTKNV login = db.tblTKNVs.SingleOrDefault(n => n.TAIKHOAN == taikhoan && n.MATKHAU == matkhau && n.IS_DELETE == false && n.IS_ACTIVE == true);
            if (login != null)
            {
                //đăng nhập tự động lưu giờ checkin
                tblBANGCONG bangCong = new tblBANGCONG();
                bangCong.GIOVAO = DateTime.Now;
                bangCong.ID_NHANVIEN = login.ID_NHANVIEN;
                //bangCong.NGAYCONGHIENTAI = DateTime.Now.Date; // Lấy ngày hiện tại
                db.tblBANGCONGs.Add(bangCong);
                db.SaveChanges();

                //lưu thông tin đăng nhập và chuyển hướng đến trang chủ
                Session["LOGIN"] = login;
                Session["TaiKhoan"] = login.TAIKHOAN;
                Session["MatKhau"] = login.MATKHAU;
                Session["MaNhanVIen"] = login.ID_NHANVIEN;
                Session["Role"] = login.Role;
                int NhanVienID = Int32.Parse(login.ID_NHANVIEN.ToString());
                tblNHANVIEN user = db.tblNHANVIENs.SingleOrDefault(n => n.ID_NHANVIEN == NhanVienID);
                
                Session["User"] = user;    
                Session["ID_NHANVIEN"] = user.ID_NHANVIEN;
                Session["HOTEN"] = user.HOTEN;
                Session["CCCD"] = user.CCCD;
                Session["GIOITINH"] = user.GIOITINH;
                Session["SDT"] = user.SDT;
                Session["QUEQUAN"] = user.QUEQUAN;
                Session["DIACHI"] = user.DIACHI;
                Session["DANTOC"] = user.DANTOC;
                Session["NGAYSINH"] = user.NGAYSINH;
                Session["LUONG_CB"] = user.LUONG_CB;
                Session["EMAIL"] = user.EMAIL;
                Session["ID_TRINHDO"] = user.tblTRINHDO.TEN_TD;
                Session["ID_LUONG"] = user.ID_LUONG;
                Session["ID_PHONGBAN"] = user.tblPHONGBAN.TENPB;
                Session["ID_CHUCVU"] = user.tblCHUCVU.TENCHUCVU;

                return RedirectToAction("index", "Staff");

            }

            ViewBag.ThongBao = "Sai tên tài khoản hoặc mật khẩu";
            return View();
        }

        //đăng xuất
        public ActionResult Logout()
        {
            // Lấy thông tin người dùng đã đăng nhập từ Session
            tblTKNV login = Session["LOGIN"] as tblTKNV;

            if (login != null)
            {
                // Xóa tất cả các session
                Session.Clear();

                // Tạo thời gian logout
                tblBANGCONG bangCong = new tblBANGCONG();
                bangCong.GIORA = DateTime.Now;
                bangCong.ID_NHANVIEN = login.ID_NHANVIEN;

                // Thêm đối tượng mới vào CSDL
                db.tblBANGCONGs.Add(bangCong);
                db.SaveChanges();
            }

            return RedirectToAction("login", "Staff");
        }
        [HttpGet]
        public ActionResult Quenmk()
        {
            return View();
        }
        //quên mật khẩu
        [HttpPost]
        public ActionResult Quenmk(string email)
        {
            string email1 = email;
            //kiểm tra email có tồn tại ko
            var e = db.tblTKNVs.FirstOrDefault(n => n.TAIKHOAN == email1 && n.IS_DELETE == false && n.IS_ACTIVE == true);
            if (e != null)
            {
                //tiến hành gửi email
                var mail = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("ungdoducthanh01@gmail.com", "ncuoqgfcffdmnqyz"),
                    EnableSsl = true
                };

                //tạo mail
                var msg = new MailMessage();
                msg.From = new MailAddress("ungdoducthanh01@gmail.com");
                msg.ReplyToList.Add("ungdoducthanh01@gmail.com");
                msg.To.Add(new MailAddress(e.TAIKHOAN));
                msg.Subject = ("Thông báo việc thay đổi mật khẩu của nhân sự");
                string mk = RandomMK();
                msg.Body = "Mật khẩu mới là: " + mk;
                e.MATKHAU = mk;
                e.UPDATE_DATE = DateTime.Now;
                e.IS_ACTIVE = true;
                e.IS_DELETE = false;
                db.Entry(e).State = EntityState.Modified;
                db.SaveChanges();
                mail.Send(msg);
                return RedirectToAction("login");
            }
            else
            {
                ViewBag.ThongBao = "Email hoặc tài khoản đăng nhập không đúng";
                return View("quenmk");
            }
        }

        public string RandomMK()
        {
            //tạo hàm random mk
            string Numrd_str;
            Random rd = new Random();
            Numrd_str = rd.Next(100000, 1000000).ToString();
            return Numrd_str;
        }

        //đổi mật khẩu
        public ActionResult Doimk()
        {
                return View();
        }
        [HttpPost]
        public ActionResult ThayDoiMatKhau(string MatKhauCu, string MatKhauMoi)
        {
            //Kiểm tra tài khoản đã đăng nhập vào:

            string ID = Session["TaiKhoan"].ToString();
            tblTKNV login = db.tblTKNVs.FirstOrDefault(n => n.TAIKHOAN == ID && n.IS_DELETE == false && n.IS_ACTIVE == true);
            if (login.MATKHAU != MatKhauCu)
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác";
                return RedirectToAction("doimk", "Staff");

            }
            //Lưu mật khẩu mới
            Session["MatKhau"] = MatKhauMoi;
            login.MATKHAU = MatKhauMoi;
            login.UPDATE_DATE = DateTime.Now;
            //Lệnh này đánh dấu đối tượng login đã được sửa đổi trong database
            db.Entry(login).State = EntityState.Modified;
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        //trang thông tin cá nhân
        public ActionResult Index()
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            int ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());
            tblNHANVIEN nhanvien = db.tblNHANVIENs.SingleOrDefault(n => n.ID_NHANVIEN == ID_NHANVIEN && n.IS_DELETE == false && n.IS_ACTIVE == true);
            if (nhanvien == null)
            {
                Response.StatusCode = 404;
            }
            ViewBag.nhanvien = nhanvien;
            return View();
        }
        [HttpPost]
        public ActionResult UpdateProfile(string sdt, string diachi, string quequan)
        {
            int ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());
            tblNHANVIEN nv = db.tblNHANVIENs.Find(ID_NHANVIEN);
            if (nv == null)
            {
                Response.StatusCode = 404;
            }
            nv.SDT = sdt;
            nv.DIACHI = diachi;
            nv.QUEQUAN = quequan;
            db.SaveChanges();
            return Json(JsonRequestBehavior.AllowGet);
        }

        //hiển thị danh sách nhân sự
        public ActionResult Loadnv()
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            var pban = db.tblPHONGBANs;
            ViewBag.tenpb = pban.ToList();
            var cvu = db.tblCHUCVUs;
            ViewBag.cvu = cvu.ToList();
            var tdo = db.tblTRINHDOes;
            ViewBag.tdo = tdo.ToList();
            var hsl = db.tblLUONGs;
            ViewBag.hsl = hsl.ToList();

            var role = Session["Role"].ToString();
            ViewBag.IsAllowAddStaff = string.Equals(role, "admin"); 
            return View();
        }
        [HttpGet]
        public JsonResult DsNhanvien(string searchName, int currentPage = 1, int pageSize = 5)
        {
            var role = Session["Role"]?.ToString() ?? string.Empty;
            // truy vấn linq thông tin nvien
            var nhanvien = (from nv in db.tblNHANVIENs
                            join cv in db.tblCHUCVUs on nv.ID_CHUCVU equals cv.ID_CHUCVU
                            join pb in db.tblPHONGBANs on nv.ID_PHONGBAN equals pb.ID_PHONGBAN
                            join td in db.tblTRINHDOes on nv.ID_TRINHDO equals td.ID_TRINHDO
                            where (string.IsNullOrEmpty(searchName) || nv.HOTEN.ToLower().Contains(searchName.ToLower()))
                            && nv.IS_ACTIVE == true && nv.IS_DELETE == false
                            orderby cv.ID_CHUCVU ascending
                            select new dsNhanvien
                            {
                                ID = nv.ID_NHANVIEN,
                                HOTEN = nv.HOTEN,
                                NGAYSINH = nv.NGAYSINH.ToString(),
                                GIOITINH = (bool)nv.GIOITINH,
                                SDT = nv.SDT,
                                TENPB = pb.TENPB,
                                TENCV = cv.TENCHUCVU,
                                TD = td.TEN_TD,
                                IsActive = (bool)nv.IS_ACTIVE,
                                IsDelete = (bool)nv.IS_DELETE,
                                IsAllowEdit = role == "admin",
                                IsAllowDelete = role == "admin",
                            }).ToList();
            int totalCount = nhanvien.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var nvien = nhanvien.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            return Json(new
            {
                data = nvien,
                totalPages = totalPages,
                currentPage = currentPage,
                pageSize = pageSize
            }, JsonRequestBehavior.AllowGet);
        }

        //xuất file excel danh sách nhân viên
        public ActionResult ExportToExcel()
        {
            //truy vấn linq thông tin nhân viên
            var nhanvien = (from nv in db.tblNHANVIENs
                            join cv in db.tblCHUCVUs on nv.ID_CHUCVU equals cv.ID_CHUCVU
                            join pb in db.tblPHONGBANs on nv.ID_PHONGBAN equals pb.ID_PHONGBAN
                            join td in db.tblTRINHDOes on nv.ID_TRINHDO equals td.ID_TRINHDO
                            where nv.IS_ACTIVE == true && nv.IS_DELETE == false
                            orderby cv.ID_CHUCVU ascending
                            select new dsNhanvien
                            {
                                ID = nv.ID_NHANVIEN,
                                HOTEN = nv.HOTEN,
                                NGAYSINH = nv.NGAYSINH.ToString(),
                                GIOITINH = (bool)nv.GIOITINH,
                                SDT = nv.SDT,
                                TENPB = pb.TENPB,
                                TENCV = cv.TENCHUCVU,
                                TD = td.TEN_TD,
                                CCCD = nv.CCCD,
                                QUE = nv.QUEQUAN,
                                DANTOC = nv.DANTOC,
                                EMAIL = nv.EMAIL,
                                Luong_CB = (double)nv.LUONG_CB,
                                IsActive = (bool)nv.IS_ACTIVE,
                                IsDelete = (bool)nv.IS_DELETE
                            }).ToList();

            //tạo file Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Danh sách nhân viên đang làm việc");

            //thiết lập các giá trị cho header của file Excel
            worksheet.Cells[1, 1].Value = "Mã nhân viên";
            worksheet.Cells[1, 2].Value = "Họ tên";
            worksheet.Cells[1, 3].Value = "Ngày sinh";
            worksheet.Cells[1, 4].Value = "Giới tính";
            worksheet.Cells[1, 5].Value = "Số điện thoại";
            worksheet.Cells[1, 6].Value = "Phòng ban";
            worksheet.Cells[1, 7].Value = "Chức vụ";
            worksheet.Cells[1, 8].Value = "Trình độ";
            worksheet.Cells[1, 9].Value = "Số căn cước";
            worksheet.Cells[1, 10].Value = "Quê quán";
            worksheet.Cells[1, 11].Value = "Dân tộc";
            worksheet.Cells[1, 12].Value = "Địa chỉ Email";
            worksheet.Cells[1, 13].Value = "Lương cơ bản";
            //thiết lập định dạng cho header
            using (var range = worksheet.Cells["A1:M1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Font.Color.SetColor(Color.Black);
            }

            //đổ dữ liệu từ danh sách nhân viên vào file Excel
            int rowIndex = 2;
            foreach (var item in nhanvien)
            {
                worksheet.Cells[rowIndex, 1].Value = item.ID;
                worksheet.Cells[rowIndex, 2].Value = item.HOTEN;
                worksheet.Cells[rowIndex, 3].Value = item.NGAYSINH;
                worksheet.Cells[rowIndex, 4].Value = item.GIOITINH ? "Nam" : "Nữ";
                worksheet.Cells[rowIndex, 5].Value = item.SDT;
                worksheet.Cells[rowIndex, 6].Value = item.TENPB;
                worksheet.Cells[rowIndex, 7].Value = item.TENCV;
                worksheet.Cells[rowIndex, 8].Value = item.TD;
                worksheet.Cells[rowIndex, 9].Value = item.CCCD;
                worksheet.Cells[rowIndex, 10].Value = item.QUE;
                worksheet.Cells[rowIndex, 11].Value = item.DANTOC;
                worksheet.Cells[rowIndex, 12].Value = item.EMAIL;
                worksheet.Cells[rowIndex, 13].Value = item.Luong_CB;

                worksheet.Cells[rowIndex, 13].Style.Numberformat.Format = "#,##0";

                rowIndex++;
            }

            //tự động căn chỉnh lại các cột trong file Excel
            worksheet.Cells.AutoFitColumns();

            //ghi file Excel và trả về dưới dạng file download
            var memoryStream = new MemoryStream();
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;
            string fileName = "Danh sách nhân viên.xlsx";
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [HttpPost]
        public JsonResult AddNhanvien(string name, string email, DateTime birthdate, int gender, string phone, string cccd, string addr, string country, string region, int salary, int position, int room, int level)
        {
            try
            {
                var nv = new tblNHANVIEN();
                nv.HOTEN = name;
                nv.EMAIL = email;
                nv.SDT = phone;
                nv.NGAYSINH = birthdate;
                nv.CCCD = cccd;
                nv.DIACHI = addr;
                nv.QUEQUAN = country;
                nv.DANTOC = region;
                nv.LUONG_CB = salary;
                nv.ID_CHUCVU = position;
                nv.ID_PHONGBAN = room;
                nv.ID_TRINHDO = level;
                nv.IS_ACTIVE = true;
                nv.IS_DELETE = false;
                nv.GIOITINH = gender == 1;
                nv.CREATE_DATE = DateTime.Now;
                db.tblNHANVIENs.Add(nv);
                db.SaveChanges();

                var idnvien = nv.ID_NHANVIEN;

                var phucap = new tblNHANVIEN_PHUCAP()
                {
                    ID_NHANVIEN = idnvien,
                    SOTIEN = 500000,
                    NOIDUNG = "Tiền phụ cấp nhân viên",
                    CREATE_DATE = DateTime.Now,
                    IS_DELETE = false,
                    IS_ACTIVE = true
                };
                db.tblNHANVIEN_PHUCAP.Add(phucap);

                var baohiem = new tblBAOHIEM()
                {
                    ID_NHANVIEN = idnvien,
                    MUC_DONG = 500000,
                    CREATE_DATE = DateTime.Now,
                    IS_DELETE = false,
                    IS_ACTIVE = true
                };
                db.tblBAOHIEMs.Add(baohiem);
               
                var tk = new tblTKNV()
                {
                    ID_NHANVIEN = idnvien,
                    TAIKHOAN = nv.EMAIL,
                    MATKHAU = nv.SDT,
                    Role = "user",
                    IS_DELETE = false,
                    IS_ACTIVE = true
                };
                db.tblTKNVs.Add(tk);
                db.SaveChanges();

                // Gửi email thông báo tới nhân viên mới
                var senderEmail = "ungdoducthanh01@gmail.com";
                var senderPassword = "ncuoqgfcffdmnqyz";
                var subject = "Thông tin tài khoản nhân viên";
                var body = $"Xin chào {name},\n\nCông ty xin gửi tới bạn thông tin đăng nhập như sau:\n\nTài khoản: {email}\nMật khẩu: {phone}\n\nHãy sử dụng thông tin này để đăng nhập vào hệ thống công ty.\n\nTrân trọng!";

                var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage(senderEmail, email, subject, body);
                smtpClient.Send(mailMessage);

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LayThongTinNV(long id)
        {
            try
            {
                var nhanvien = db.tblNHANVIENs.Where(nv => nv.ID_NHANVIEN == id).Select(nv => new
                {
                    nv.ID_NHANVIEN,
                    nv.HOTEN,
                    nv.CCCD,
                    nv.GIOITINH,
                    nv.SDT,
                    nv.QUEQUAN,
                    nv.DIACHI,
                    nv.DANTOC,
                    NGAYSINH = nv.NGAYSINH.ToString(),
                    nv.LUONG_CB,
                    nv.EMAIL,
                    nv.ID_TRINHDO,
                    nv.ID_PHONGBAN,
                    nv.ID_CHUCVU,
                    isactive = nv.IS_ACTIVE,
                    isdelete =nv.IS_DELETE

                }).FirstOrDefault();
                if (nhanvien == null)
                    throw new Exception();
                else
                    return Json(new { success = true, data = nhanvien }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult EditNhanvien(long id, string name, string email, DateTime birthdate, int gender, string phone, string cccd, string addr, string country, string region, int salary, int position, int room, int level)
        {
            try
            {
                tblNHANVIEN nv = db.tblNHANVIENs.Find(id);
                if (nv != null)
                {
                    nv.HOTEN = name;
                    nv.EMAIL = email;
                    nv.SDT = phone;
                    nv.NGAYSINH = birthdate;
                    nv.CCCD = cccd;
                    nv.DIACHI = addr;
                    nv.QUEQUAN = country;
                    nv.DANTOC = region;
                    nv.LUONG_CB = salary;
                    nv.ID_CHUCVU = position;
                    nv.ID_PHONGBAN = room;
                    nv.ID_TRINHDO = level;
                    nv.UPDATE_DATE = DateTime.Now;
                    nv.GIOITINH = gender == 1;
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("Không tìm thấy nhân viên với id đã cho");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult XoaNhanVien(long id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    tblNHANVIEN nv = db.tblNHANVIENs.Find(id);
                    if (nv != null)
                    {
                        nv.IS_ACTIVE = false;
                        nv.IS_DELETE = true;
                        nv.UPDATE_DATE = DateTime.Now;
                        var tknv = db.tblTKNVs.FirstOrDefault(t => t.ID_NHANVIEN == id);
                        if (tknv != null)
                        {
                            tknv.IS_ACTIVE = false;
                            tknv.IS_DELETE = true;
                        }
                        db.SaveChanges();
                    }
                    transaction.Commit();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Bangcong(int? month)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }

            string query = @"
        SELECT
            NV.ID_NHANVIEN AS ID_NHANVIEN,
            NV.HOTEN AS HoTen,
            COUNT(DISTINCT CONVERT(DATE, BC.GIOVAO)) AS SoNgayVao,
            SUM(CASE WHEN CONVERT(TIME, BC.GIOVAO) > '08:00:00' THEN 1 ELSE 0 END) AS NgayDiMuon
        FROM
            tblNHANVIEN NV
            LEFT JOIN (
                SELECT
                    ID_NHANVIEN,
                    MIN(GIOVAO) AS GIOVAO
                FROM
                    tblBANGCONG
                GROUP BY
                    ID_NHANVIEN,
                    CONVERT(DATE, GIOVAO)
            ) BC ON NV.ID_NHANVIEN = BC.ID_NHANVIEN
        WHERE
            NV.IS_DELETE = 0
            AND NV.IS_ACTIVE = 1";

            if (month.HasValue)
            {
                query += $" AND MONTH(BC.GIOVAO) = {month.Value}";
            }

            query += @"
        GROUP BY
            NV.ID_NHANVIEN, NV.HOTEN";

            var data = db.Database.SqlQuery<SoNgayCongModel>(query).ToList();

            var months = db.tblBANGCONGs
                .Where(x => x.GIOVAO != null)
                .Select(x => x.GIOVAO.Value.Month)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Months = new SelectList(months);

            ViewBag.data = data;
            return View();
        }

        public ActionResult PersonalScoreBoard(int? month)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }

            int ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());

            string query = @"
        SELECT
            NV.ID_NHANVIEN AS ID_NHANVIEN,
            NV.HOTEN AS HoTen,
            COUNT(DISTINCT CONVERT(DATE, BC.GIOVAO)) AS SoNgayVao,
            SUM(CASE WHEN CONVERT(TIME, BC.GIOVAO) > '08:00:00' THEN 1 ELSE 0 END) AS NgayDiMuon
        FROM
            tblNHANVIEN NV
            LEFT JOIN (
                SELECT
                    ID_NHANVIEN,
                    MIN(GIOVAO) AS GIOVAO
                FROM
                    tblBANGCONG
                WHERE
                    ID_NHANVIEN = @ID_NHANVIEN
                GROUP BY
                    ID_NHANVIEN,
                    CONVERT(DATE, GIOVAO)
            ) BC ON NV.ID_NHANVIEN = BC.ID_NHANVIEN
        WHERE
            NV.IS_DELETE = 0
            AND NV.IS_ACTIVE = 1
            AND NV.ID_NHANVIEN = @ID_NHANVIEN";

            if (month.HasValue)
            {
                query += " AND MONTH(BC.GIOVAO) = @SelectedMonth";
            }
            else
            {
                query += " AND MONTH(BC.GIOVAO) = MONTH(GETDATE())";
            }

            query += @"
        GROUP BY
            NV.ID_NHANVIEN, NV.HOTEN";

            SqlParameter[] parameters =
            {
        new SqlParameter("@ID_NHANVIEN", ID_NHANVIEN),
        new SqlParameter("@SelectedMonth", month ?? (object)DBNull.Value)
    };

            var data = db.Database.SqlQuery<SoNgayCongModel>(query, parameters).ToList();

            var months = db.tblBANGCONGs
                .Where(x => x.GIOVAO != null)
                .Where(x => x.ID_NHANVIEN == ID_NHANVIEN)
                .Select(x => x.GIOVAO.Value.Month)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Months = new SelectList(months);

            ViewBag.data = data;
            return View();
        }


        public ActionResult Tinhluong(string name, int page = 1, int pageSize = 5)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            DateTime currentDate = DateTime.Now;
            string query = @"SELECT 
                          nv.ID_NHANVIEN, 
                          nv.HOTEN, 
                          nv.LUONG_CB, 
                          bh.MUC_DONG, 
                          np.SOTIEN, 
                          COUNT(DISTINCT DAY(bc.GIOVAO)) AS SoNgayChamCong,
                          (nv.LUONG_CB / 
                            (SELECT COUNT(*)
                             FROM (
                               SELECT DATEADD(day, number, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)) AS dt
                               FROM master..spt_values
                               WHERE type = 'P'
                               AND number < DATEDIFF(day, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0), DATEADD(month, DATEDIFF(month, -1, GETDATE()), 0))
                             ) AS dates
                             WHERE DATEPART(dw, dt) NOT IN (1, 7)
                            ) * COUNT(DISTINCT DAY(bc.GIOVAO))) + bh.MUC_DONG + np.SOTIEN AS TongLuong
                        FROM tblNHANVIEN nv
                        INNER JOIN tblNHANVIEN_PHUCAP np ON nv.ID_NHANVIEN = np.ID_NHANVIEN
                        INNER JOIN tblBAOHIEM bh ON nv.ID_NHANVIEN = bh.ID_NHANVIEN
                        INNER JOIN tblBANGCONG bc ON nv.ID_NHANVIEN = bc.ID_NHANVIEN
                        WHERE bc.GIOVAO IS NOT NULL 
                              AND MONTH(bc.GIOVAO) = MONTH(GETDATE()) 
                              AND YEAR(bc.GIOVAO) = YEAR(GETDATE())
                              AND nv.IS_ACTIVE = 1
                              AND nv.IS_DELETE = 0
                        GROUP BY nv.ID_NHANVIEN, nv.HOTEN, nv.LUONG_CB, bh.MUC_DONG, np.SOTIEN;";
            var data = db.Database.SqlQuery<LuongNhanVien>(query).ToList();
            if (!string.IsNullOrEmpty(name))
            {
                data = data.Where(x => x.HoTen.Contains(name)).ToList();
            }
            int totalCount = data.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            data = data.OrderBy(x => x.ID_NHANVIEN).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.query = data;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View();
        }

        public ActionResult PersonalSalary(int? selectedMonth)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }

            int ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());

            // Retrieve the list of distinct months with attendance data for the employee
            var months = db.tblBANGCONGs
                .Where(x => x.GIOVAO != null && x.ID_NHANVIEN == ID_NHANVIEN)
                .Select(x => x.GIOVAO.Value.Month)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Months = new SelectList(months);

            DateTime currentDate = DateTime.Now;

            // Set the selected month to the current month if none is selected
            if (!selectedMonth.HasValue)
            {
                selectedMonth = currentDate.Month;
            }

            // Filter the salary calculation query by the selected month or all months if none is selected
            string query = @"SELECT 
                            nv.ID_NHANVIEN, 
                            nv.HOTEN, 
                            nv.LUONG_CB, 
                            bh.MUC_DONG, 
                            np.SOTIEN, 
        COUNT(DISTINCT DAY(bc.GIOVAO)) AS SoNgayChamCong,
        (nv.LUONG_CB / 
            (SELECT COUNT(*)
             FROM (
               SELECT DATEADD(day, number, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)) AS dt
               FROM master..spt_values
               WHERE type = 'P'
               AND number < DATEDIFF(day, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0), DATEADD(month, DATEDIFF(month, -1, GETDATE()), 0))
             ) AS dates
             WHERE DATEPART(dw, dt) NOT IN (1, 7)
            ) * COUNT(DISTINCT DAY(bc.GIOVAO))) + bh.MUC_DONG + np.SOTIEN AS TongLuong
            FROM tblNHANVIEN nv
            INNER JOIN tblNHANVIEN_PHUCAP np ON nv.ID_NHANVIEN = np.ID_NHANVIEN
            INNER JOIN tblBAOHIEM bh ON nv.ID_NHANVIEN = bh.ID_NHANVIEN
            LEFT JOIN tblBANGCONG bc ON nv.ID_NHANVIEN = bc.ID_NHANVIEN
            WHERE nv.ID_NHANVIEN = @ID_NHANVIEN
                AND (MONTH(bc.GIOVAO) = @SelectedMonth OR @SelectedMonth IS NULL)
                AND YEAR(bc.GIOVAO) = YEAR(GETDATE())
            GROUP BY nv.ID_NHANVIEN, nv.HOTEN, nv.LUONG_CB, bh.MUC_DONG, np.SOTIEN;";

            // Pass the selected month parameter to the SQL query
            var data = db.Database.SqlQuery<LuongNhanVien>(query,
                new SqlParameter("@SelectedMonth", selectedMonth ?? (object)DBNull.Value),
                new SqlParameter("@ID_NHANVIEN", ID_NHANVIEN))
                .ToList();

            ViewBag.query = data;

            return View();
        }


        public ActionResult ExportToExcelSalary(string name)
        {
            DateTime currentDate = DateTime.Now;
            string query = @"SELECT 
                          nv.ID_NHANVIEN, 
                          nv.HOTEN, 
                          nv.LUONG_CB, 
                          bh.MUC_DONG, 
                          np.SOTIEN, 
                          COUNT(DISTINCT DAY(bc.GIOVAO)) AS SoNgayChamCong,
                          (nv.LUONG_CB / 
                            (SELECT COUNT(*)
                             FROM (
                               SELECT DATEADD(day, number, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)) AS dt
                               FROM master..spt_values
                               WHERE type = 'P'
                               AND number < DATEDIFF(day, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0), DATEADD(month, DATEDIFF(month, -1, GETDATE()), 0))
                             ) AS dates
                             WHERE DATEPART(dw, dt) NOT IN (1, 7)
                            ) * COUNT(DISTINCT DAY(bc.GIOVAO))) + bh.MUC_DONG + np.SOTIEN AS TongLuong
                        FROM tblNHANVIEN nv
                        INNER JOIN tblNHANVIEN_PHUCAP np ON nv.ID_NHANVIEN = np.ID_NHANVIEN
                        INNER JOIN tblBAOHIEM bh ON nv.ID_NHANVIEN = bh.ID_NHANVIEN
                        INNER JOIN tblBANGCONG bc ON nv.ID_NHANVIEN = bc.ID_NHANVIEN
                        WHERE bc.GIOVAO IS NOT NULL 
                              AND MONTH(bc.GIOVAO) = MONTH(GETDATE()) 
                              AND YEAR(bc.GIOVAO) = YEAR(GETDATE())
                              AND nv.IS_ACTIVE = 1
                              AND nv.IS_DELETE = 0
                        GROUP BY nv.ID_NHANVIEN, nv.HOTEN, nv.LUONG_CB, bh.MUC_DONG, np.SOTIEN;";
            var data = db.Database.SqlQuery<LuongNhanVien>(query).ToList();
            if (!string.IsNullOrEmpty(name))
            {
                data = data.Where(x => x.HoTen.Contains(name)).ToList();
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tinhluong");

            // Thiết lập các giá trị cho header của file Excel
            worksheet.Cells[1, 1].Value = "ID Nhân viên";
            worksheet.Cells[1, 2].Value = "Họ tên";
            worksheet.Cells[1, 3].Value = "Lương cơ bản";
            worksheet.Cells[1, 4].Value = "Số công";
            worksheet.Cells[1, 5].Value = "Phụ cấp";
            worksheet.Cells[1, 6].Value = "Bảo hiểm";
            worksheet.Cells[1, 7].Value = "Tổng lương";
            //thiết lập định dạng cho header
            using (var range = worksheet.Cells["A1:G1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Font.Color.SetColor(Color.Black);
            }
            // Đổ dữ liệu từ danh sách nhân viên vào file Excel
            int rowIndex = 2;
            foreach (var item in data)
            {
                worksheet.Cells[rowIndex, 1].Value = item.ID_NHANVIEN;
                worksheet.Cells[rowIndex, 2].Value = item.HoTen;
                worksheet.Cells[rowIndex, 3].Value = item.Luong_CB;
                worksheet.Cells[rowIndex, 4].Value = item.SoNgayChamCong;
                worksheet.Cells[rowIndex, 5].Value = item.Muc_Dong;
                worksheet.Cells[rowIndex, 6].Value = item.SoTien;
                worksheet.Cells[rowIndex, 7].Value = item.TongLuong;

                // Định dạng các trường dữ liệu
                worksheet.Cells[rowIndex, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[rowIndex, 6].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[rowIndex, 7].Style.Numberformat.Format = "#,##0";
                rowIndex++;
            }

            // Tự động căn chỉnh lại các cột trong file Excel
            worksheet.Cells.AutoFitColumns();

            // Lưu file Excel vào một MemoryStream
            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            string fileName = "Danh sách lương nhân viên.xlsx";
            // Trả về file Excel như một FileResult
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public ActionResult CapNhatLuong(double sotien, double mucdong,double luongcb, int id_nhanvien)
        {
            if (ModelState.IsValid)
            {
                var phucap = db.tblNHANVIEN_PHUCAP.FirstOrDefault(x => x.ID_NHANVIEN == id_nhanvien);
                if (phucap != null)
                {
                    phucap.SOTIEN = sotien;
                    phucap.UPDATE_DATE = DateTime.Now;
                }

                var baohiem = db.tblBAOHIEMs.FirstOrDefault(x => x.ID_NHANVIEN == id_nhanvien);
                if (baohiem != null)
                {
                    baohiem.MUC_DONG = mucdong;
                    baohiem.UPDATE_DATE = DateTime.Now;
                }
                var nhanvien = db.tblNHANVIENs.FirstOrDefault(x => x.ID_NHANVIEN == id_nhanvien);
                if(nhanvien != null)
                {
                    nhanvien.LUONG_CB = luongcb;
                    nhanvien.UPDATE_DATE = DateTime.Now;
                }
                db.SaveChanges();

                return RedirectToAction("tinhluong");
            }
            else
            {
                // Trả về view với thông báo lỗi nếu dữ liệu không hợp lệ
                return View();
            }
        }


        public ActionResult Report()
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            int ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());
            tblNHANVIEN nhanvien = db.tblNHANVIENs.SingleOrDefault(n => n.ID_NHANVIEN == ID_NHANVIEN && n.IS_DELETE == false && n.IS_ACTIVE == true);
            if (nhanvien == null)
            {
                Response.StatusCode = 404;
            }
            ViewBag.nhanvien = nhanvien;
            return View();
        }
        [HttpPost]
        public ActionResult SendReport(tblBAOCAONHANVIEN baocao, string noidungcongviec)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(noidungcongviec)) // Kiểm tra nội dung công việc
                {
                    ModelState.AddModelError("noidungcongviec", "Bạn cần điền nội dung công việc");
                    return View(baocao);
                }
                baocao.ID_NHANVIEN = Int32.Parse(Session["ID_NHANVIEN"].ToString());
                baocao.NGAY_BAOCAO = DateTime.Now;
                baocao.NOIDUNG = noidungcongviec;
                baocao.CREATE_DATE = DateTime.Now;
                baocao.IS_ACTIVE = true;
                baocao.IS_DELETE = false;
                db.tblBAOCAONHANVIENs.Add(baocao);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(baocao);
        }


        public ActionResult LstPhongban(string name)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            var result = from p in db.tblPHONGBANs
                         join n in db.tblNHANVIENs.Where(x => x.IS_DELETE == false && x.IS_ACTIVE == true) on p.ID_PHONGBAN equals n.ID_PHONGBAN into gj
                         from subNhanVien in gj.DefaultIfEmpty()
                         where p.IS_DELETE == false && p.IS_ACTIVE == true &&
                         (string.IsNullOrEmpty(name) || p.TENPB.Contains(name))
                         group subNhanVien by new { p.ID_PHONGBAN, p.TENPB } into g
                         select new dsPhongban
                         {
                             ID = g.Key.ID_PHONGBAN,
                             Name = g.Key.TENPB,
                             SoNV = g.Count(x => x != null)
                         };
            ViewBag.result = result;
            return View();
        }
        public ActionResult DetailPhongban(int id,string searchname)
        {
            var nvien = db.tblNHANVIENs.Find(id);
            var query = from nv in db.tblNHANVIENs
                        join pb in db.tblPHONGBANs on nv.ID_PHONGBAN equals pb.ID_PHONGBAN
                        where pb.ID_PHONGBAN == id && nv.IS_ACTIVE == true && nv.IS_DELETE == false
                        && (string.IsNullOrEmpty(searchname) || nv.HOTEN.ToLower().Contains(searchname.ToLower()))
                        select new dsNhanvien
                        {
                            ID = nv.ID_NHANVIEN,
                            HOTEN = nv.HOTEN,
                            EMAIL = nv.EMAIL,
                            GIOITINH = (bool)nv.GIOITINH,
                            SDT = nv.SDT,
                        };
            ViewBag.query = query;
            return View();
        }

        public ActionResult DetailReport(int? month)
        {
            if (Session["TaiKhoan"] == null)
            {
                return RedirectToAction("login", "Staff");
            }
            // Lấy danh sách các tháng từ tblBAOCAONHANVIEN
            var months = db.tblBAOCAONHANVIENs
                            .Where(x => x.NGAY_BAOCAO != null)
                            .Select(x => x.NGAY_BAOCAO.Value.Month)
                            .Distinct()
                            .OrderBy(x => x)
                            .ToList();

            // Thêm danh sách các tháng vào ViewBag để hiển thị trong dropdownlist
            ViewBag.Months = new SelectList(months);

            // Lấy dữ liệu báo cáo của nhân viên
            var query = db.tblNHANVIENs
    .Join(
        db.tblBAOCAONHANVIENs
            .Where(x => x.NGAY_BAOCAO != null
                && (!month.HasValue || x.NGAY_BAOCAO.Value.Month == month.Value)
            ),
        nv => nv.ID_NHANVIEN,
        bc => bc.ID_NHANVIEN,
        (nv, bc) => new BaoCaoViewModel
        {
            ID_NHANVIEN = nv.ID_NHANVIEN,
            HOTEN = nv.HOTEN,
            NGAY_BAOCAO = bc.NGAY_BAOCAO,
            NOIDUNG = bc.NOIDUNG,
            IS_ACTIVE = (bool)nv.IS_ACTIVE,
            IS_DELETE = (bool)nv.IS_DELETE
        }
    )
    .ToList() 
    .Where(nv => nv.IS_ACTIVE == true && nv.IS_DELETE == false); 

            ViewBag.query = query;
            return View();

        }

    }
}