using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChamCong.Models
{
    public class BangCongModel : tblBANGCONG
    {
        public DateTime? NGAYVAO { get; set; }

        public BangCongModel(tblBANGCONG bc)
        {
            ID_BANGCONG = bc.ID_BANGCONG;
            NGAYVAO = GIOVAO.HasValue ? GIOVAO.Value.Date : (DateTime?)null;
            GIOVAO = bc.GIOVAO;
            GIORA = bc.GIORA;
            ID_LOAICONG = bc.ID_LOAICONG;
            ID_NHANVIEN = bc.ID_NHANVIEN;
        }
    }
}