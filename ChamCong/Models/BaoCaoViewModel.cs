using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamCong.Models
{
    public class BaoCaoViewModel
    {
        public long ID_NHANVIEN { get; set; }
        public string HOTEN { get; set; }
        public DateTime? NGAY_BAOCAO { get; set; }
        public string NOIDUNG { get; set; }
        public bool IS_ACTIVE { get; set; }
        public bool IS_DELETE { get; set; }
    }
}