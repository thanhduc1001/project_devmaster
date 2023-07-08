function showInfo(id) {
    idnhanvien = id;
    $.ajax({
        type: "GET",
        url: `/Staff/LayThongTinNV?id=${id}`,
        success: function (response) {
            $('#nameModal1').val(response.data.HOTEN);
            $('#emailModal1').val(response.data.EMAIL);
            $('#birthdateModal1').val(response.data.NGAYSINH);
            $('#genderModal1').val(response.data.GIOITINH ? 1 : 2);
            $('#phoneModal1').val(response.data.SDT);
            $('#identityModal1').val(response.data.CCCD);
            $('#addrModal1').val(response.data.DIACHI);
            $('#countryModal1').val(response.data.QUEQUAN);
            $('#regionModal1').val(response.data.DANTOC);
            $('#salaryModal1').val(response.data.LUONG_CB);
            $('#positionModal1').val(response.data.ID_CHUCVU);
            $('#roomModal11').val(response.data.ID_PHONGBAN);
            $('#levelModal1').val(response.data.ID_TRINHDO);
        },
        error: function () {
            alert('Đã có lỗi xảy ra!');
        }
    });
}
$('#btnEdit').click(function () {
    var name = $('#nameModal1').val();
    var email = $('#emailModal1').val();
    var birthdate = $('#birthdateModal1').val();
    var gender = $('#genderModal1').val();
    var phone = $('#phoneModal1').val();
    var cccd = $('#identityModal1').val();
    var addr = $('#addrModal1').val();
    var country = $('#countryModal1').val();
    var region = $('#regionModal1').val();
    var salary = $('#salaryModal1').val();
    var position = $('#positionModal1').val();
    var room = $('#roomModal1').val();
    var level = $('#levelModal1').val();

    if (name == "") {
        alert("Họ tên không được bỏ trống!");
        return;
    }
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        alert("Vui lòng nhập email hợp lệ");
        return;
    }

    var phoneRegex = /^(0|\+84)\d{9}$/;
    if (!phoneRegex.test(phone)) {
        alert("Vui lòng nhập số điện thoại hợp lệ (10 hoặc 11 chữ số, bắt đầu bằng 0 hoặc +84)");
        return;
    }

    var identityRegex = /^\d{12}$/;
    if (!identityRegex.test(cccd)) {
        alert("Vui lòng nhập số căn cước hợp lệ (12 chữ số)");
        return;
    }

    $.ajax({
        type: "POST",
        url: "/Staff/EditNhanvien",
        data: {
            id: idnhanvien,
            name: name,
            email: email,
            birthdate: birthdate,
            gender: gender,
            phone: phone,
            cccd: cccd,
            addr: addr,
            country: country,
            region: region,
            salary: salary,
            position: position,
            room: room,
            level: level
        },
        success: function (response) {
            $('#modalEdit').modal('hide');
            alert('Sửa thông tin nhân viên thành công!');
            StaffController.dsNhanvien();
        },
        error: function (response) {
            alert('Lỗi khi sửa thông tin nhân viên: ' + response.responseText);
        }
    });
});