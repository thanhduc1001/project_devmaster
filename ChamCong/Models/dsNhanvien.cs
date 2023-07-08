using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamCong.Models
{
    public class dsNhanvien
    {
        public long ID { get; set; }
        public string HOTEN { get; set; }
        public string NGAYSINH { get; set; }
        public string SDT { get; set; }
        public string TENPB { get; set; }
        public string TENCV { get; set; }
        public bool GIOITINH { get; set; }
        public string TD { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public string CCCD { get; set; }
        public string QUE { get; set; }
        public string DIACHI { get; set; }
        public string DANTOC { get; set; }
        public string EMAIL { get; set; }
        public double Luong_CB { get; set; }
        public bool IsAllowEdit { get; set; }
        public bool IsAllowDelete { get; set; }
    }
}