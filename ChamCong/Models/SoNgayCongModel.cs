using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamCong.Models
{
    public class SoNgayCongModel
    {
        public long ID_NHANVIEN { get; set; }
        public string HoTen { get; set; }
        public int SoNgayVao { get; set; }
        public int NgayDiMuon { get; set; }
        public int VeSom { get; set; }
    }
}