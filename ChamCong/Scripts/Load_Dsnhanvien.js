var StaffController = {
    init: function () {
        StaffController.dsNhanvien('', 1, 5); // hiển thị trang đầu tiên với 5 bản ghi trên mỗi trang
    },

    dsNhanvien: function (searchName, currentPage, pageSize) {
        $.ajax({
            url: "/Staff/dsNhanvien",
            type: "GET",
            dataType: 'json',
            data: {
                searchName: searchName,
                currentPage: currentPage,
                pageSize: pageSize
            },
            success: function (response) {
                var data = response.data;
                var html = '';
                var template = $("#data-template").html();
                $.each(data, function (i, item) {
                    var ngaysinh_old = item.NGAYSINH;
                    var split = ngaysinh_old.split("-", 3);
                    var ngaysinh = split[2] + "/" + split[1] + "/" + split[0];
                    var btnEdit = '';
                    var btnDelete = '';
                    if (item.IsAllowEdit) {
                        btnEdit = `<button onclick="showInfo(${item.ID})" type="button"
                                            class="btn rounded-pill btn-outline-dark"
                                            data-bs-target="#modalEdit"
                                            data-bs-toggle="modal"
                                            data-bs-offset="0,4"
                                            data-bs-placement="top"
                                            data-bs-html="true"
                                            title="Xem chi tiết">
                                        <i class='bx bx-edit'></i>
                                    </button>`
                    }
                    if (item.IsAllowDelete) {
                        btnDelete = `<button onclick="XoaNhanVien(${item.ID})" type="button"
                                            class="btn rounded-pill btn-outline-danger"
                                            data-bs-toggle="tooltip"
                                            data-bs-offset="0,4"
                                            data-bs-placement="top"
                                            data-bs-html="true"
                                            title="Xóa">

                                        <i class='bx bxs-eraser'></i>
                                    </button>`
                    }
                    html += Mustache.render(template, {
                        ID: item.ID,
                        HOTEN: item.HOTEN,
                        GIOITINH: item.GIOITINH == true ? "Nam" : "Nữ",
                        TENCV: item.TENCV,
                        SDT: item.SDT,
                        NGAYSINH: ngaysinh,
                        IsAllowEdit: item.IsAllowEdit,
                        IsAllowDelete: item.IsAllowDelete
                    });
                });

                $('#LoadDsnv').html(html);

                // Hiển thị phân trang
                var totalRecords = response.totalRecords;
                var totalPages = response.totalPages;
                var pagination = '';
                if (totalPages > 1) {
                    pagination += '<ul class="pagination justify-content-center">';
                    if (currentPage > 1) {
                        pagination += '<li  class="page-item prev"><a class="page-link" href="javascript:void(0);" data-page="' + (currentPage - 1) + '"><i class="tf-icon bx bx-chevrons-left"></i></a></li>';
                    }
                    var currentLoopStart = 1;
                    var currentLoopEnd = totalPages;
                    if (totalPages > 5 && currentPage > 3) {
                        currentLoopStart = currentPage - 2;
                        currentLoopEnd = currentPage + 2 <= totalPages ? currentPage + 2 : totalPages;
                    }
                    for (var i = currentLoopStart; i <= currentLoopEnd; i++) {
                        if (i == currentPage) {
                            pagination += '<li class="page-item active"><a class="page-link" href="javascript:void(0);">' + i + '</a></li>';
                        } else {
                            pagination += '<li class="page-item"><a class="page-link" href="javascript:void(0);" data-page="' + i + '">' + i + '</a></li>';
                        }
                    }
                    if (currentPage < totalPages) {
                        pagination += '<li class="page-item next"><a class="page-link" href="javascript:void(0);" data-page="' + (currentPage + 1) + '"><i class="tf-icon bx bx-chevrons-right"></i></a></li>';
                    }
                    pagination += '</ul>';
                }
                $('#pagination').html(pagination);

                // Bắt sự kiện click vào trang
                $('#pagination').on('click', 'a', function (event) {
                    event.preventDefault();
                    var page = $(this).data('page');
                    StaffController.dsNhanvien(searchName, page, pageSize);
                });
            }
        });
    }
}
StaffController.init();
