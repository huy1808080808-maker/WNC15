// Hệ thống quản lý Hội viên & Sự kiện Hiệp hội
// Tự động Việt hóa toàn bộ thông báo xác thực (Validation) trên trình duyệt

document.addEventListener("DOMContentLoaded", function () {
    // 1. Hàm áp dụng thông báo tiếng Việt cho các trường form HTML5
    function attachVietnameseValidation(elements) {
        elements.forEach(function (input) {
            input.addEventListener("invalid", function (e) {
                // Kiểm tra loại lỗi validation
                if (input.validity.valueMissing) {
                    if (input.type === "checkbox" || input.type === "radio") {
                        input.setCustomValidity("Vui lòng tích chọn xác nhận để tiếp tục.");
                    } else if (input.tagName.toLowerCase() === "select") {
                        input.setCustomValidity("Vui lòng chọn một mục trong danh sách.");
                    } else {
                        input.setCustomValidity("Vui lòng không để trống trường thông tin này.");
                    }
                } else if (input.validity.typeMismatch) {
                    if (input.type === "email") {
                        input.setCustomValidity("Vui lòng nhập đúng định dạng thư điện tử (ví dụ: hoten@gmail.com).");
                    } else if (input.type === "url") {
                        input.setCustomValidity("Vui lòng nhập địa chỉ liên kết (URL) hợp lệ.");
                    } else {
                        input.setCustomValidity("Dữ liệu nhập vào không đúng định dạng yêu cầu.");
                    }
                } else if (input.validity.patternMismatch) {
                    input.setCustomValidity("Dữ liệu nhập vào không khớp với định dạng yêu cầu.");
                } else if (input.validity.rangeUnderflow) {
                    input.setCustomValidity("Giá trị phải lớn hơn hoặc bằng " + (input.min || "1") + ".");
                } else if (input.validity.rangeOverflow) {
                    input.setCustomValidity("Giá trị phải nhỏ hơn hoặc bằng " + (input.max || "") + ".");
                } else if (input.validity.tooShort) {
                    input.setCustomValidity("Vui lòng nhập tối thiểu " + input.minLength + " ký tự.");
                } else if (input.validity.tooLong) {
                    input.setCustomValidity("Vui lòng không nhập quá " + input.maxLength + " ký tự.");
                } else if (input.validity.stepMismatch) {
                    input.setCustomValidity("Giá trị đã nhập không đúng bước nhảy hợp lệ.");
                } else {
                    input.setCustomValidity("Thông tin nhập vào chưa hợp lệ.");
                }
            });

            // Khi người dùng bắt đầu gõ hoặc thay đổi giá trị, xóa thông báo lỗi để cho phép submit lại
            input.addEventListener("input", function () {
                input.setCustomValidity("");
            });
            input.addEventListener("change", function () {
                input.setCustomValidity("");
            });
        });
    }

    // Áp dụng cho toàn bộ form inputs hiện có trên trang
    var formInputs = document.querySelectorAll("input, select, textarea");
    attachVietnameseValidation(formInputs);

    // 2. Tự động đóng thông báo alert sau 5 giây nếu người dùng không bấm tắt
    var autoAlerts = document.querySelectorAll(".alert.alert-dismissible");
    autoAlerts.forEach(function (alertEl) {
        setTimeout(function () {
            var closeBtn = alertEl.querySelector(".btn-close");
            if (closeBtn) {
                closeBtn.click();
            }
        }, 5000);
    });
});
