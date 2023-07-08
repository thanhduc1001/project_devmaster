$("#btnSaveAdd").click(function () {
    var name = $('#nameModal').val();
    var email = $('#emailModal').val();
    var birthdate = $('#birthdateModal').val();
    var gender = $('#genderModal').val();
    var phone = $('#phoneModal').val();
    var cccd = $('#identityModal').val();
    var addr = $('#addrModal').val();
    var country = $('#countryModal').val();
    var region = $('#regionModal').val();
    var salary = $('#salaryModal').val();
    var position = $('#positionModal').val();
    var room = $('#roomModal').val();
    var level = $('#levelModal').val();

    // Validate input fields
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

    // Call AJAX to add new employee
    $.ajax({
        type: "POST",
        url: "/Staff/AddNhanvien",
        data: {
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
            $('#modalAdd').modal('hide');
            alert('Thêm nhân viên thành công!');
            StaffController.dsNhanvien();
        },
        error: function (response) {
            alert('Lỗi thêm nhân viên: ' + response.responseText);
        }
    });
});

$('#btnAdd').click(function () {
    $('#modalAdd').modal("show");
})

