function XoaNhanVien(id) {
    if (confirm("Bạn có chắc muốn xóa nhân viên này?")) {
        $.ajax({
            type: "POST",
            url: "/Staff/XoaNhanvien",
            data: { id: id },
            success: function (result) {
                alert("Xóa nhân viên thành công!");
                StaffController.dsNhanvien();
            },
            error: function () {
                alert("Đã có lỗi xảy ra!");
            }
        });
    }
};