using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamCong.Models
{
    public class LuongNhanVien
    {
        public long ID_NHANVIEN { get; set; }
        public string HoTen { get; set; }   
        public double Luong_CB { get; set; }
        public double Muc_Dong { get; set; }
        public int SoNgayChamCong { get; set; }
        public int SoNgayCtyLam { get; set; }
        public double SoTien { get; set; }
        public double TongLuong { get; set; }

    }
}