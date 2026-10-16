-- =============================================
-- KỊCH BẢN TẠO CƠ SỞ DỮ LIỆU VÀ DỮ LIỆU MẪU
-- ĐỒ ÁN: QUẢN LÝ HIỆP HỘI / HỘI VIÊN (WNC15)
-- =============================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyHiepHoi')
BEGIN
    CREATE DATABASE QuanLyHiepHoi;
END
GO

USE QuanLyHiepHoi;
GO

-- 1. BẢNG nguoi_dung (Tài khoản, Hội viên, Quản trị viên)
IF OBJECT_ID('dang_ky_su_kien', 'U') IS NOT NULL DROP TABLE dang_ky_su_kien;
IF OBJECT_ID('su_kien', 'U') IS NOT NULL DROP TABLE su_kien;
IF OBJECT_ID('bai_viet', 'U') IS NOT NULL DROP TABLE bai_viet;
IF OBJECT_ID('danh_muc', 'U') IS NOT NULL DROP TABLE danh_muc;
IF OBJECT_ID('nguoi_dung', 'U') IS NOT NULL DROP TABLE nguoi_dung;
GO

CREATE TABLE nguoi_dung (
    ma_nguoi_dung INT IDENTITY(1,1) PRIMARY KEY,
    ho_ten NVARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    so_dien_thoai VARCHAR(20) NULL,
    don_vi_cong_tac NVARCHAR(200) NULL,
    chuc_danh NVARCHAR(100) NULL,
    linh_vuc NVARCHAR(100) NULL,
    mat_khau_ma_hoa VARCHAR(255) NOT NULL,
    ngay_sinh DATE NULL,
    gioi_thieu NVARCHAR(MAX) NULL,
    anh_dai_dien NVARCHAR(255) NULL,
    vai_tro VARCHAR(20) NOT NULL DEFAULT 'hoi_vien',       -- 'quan_tri', 'hoi_vien'
    trang_thai VARCHAR(20) NOT NULL DEFAULT 'cho_duyet',    -- 'cho_duyet', 'hoat_dong', 'tu_choi', 'khoa'
    ly_do_tu_choi NVARCHAR(500) NULL,
    ngay_tao DATETIME DEFAULT GETDATE()
);
GO

-- 2. BẢNG danh_muc (Danh mục bài viết / tin tức)
CREATE TABLE danh_muc (
    ma_danh_muc INT IDENTITY(1,1) PRIMARY KEY,
    ten_danh_muc NVARCHAR(100) NOT NULL,
    mo_ta NVARCHAR(255) NULL
);
GO

-- 3. BẢNG bai_viet (Tin tức & Bài viết)
CREATE TABLE bai_viet (
    ma_bai_viet INT IDENTITY(1,1) PRIMARY KEY,
    ma_danh_muc INT NOT NULL,
    ma_tac_gia INT NOT NULL,
    tieu_de NVARCHAR(255) NOT NULL,
    tom_tat NVARCHAR(500) NULL,
    noi_dung NVARCHAR(MAX) NOT NULL,
    anh_dai_dien NVARCHAR(255) NULL,
    trang_thai VARCHAR(20) NOT NULL DEFAULT 'da_dang',      -- 'ban_nhap', 'da_dang', 'an'
    luot_xem INT DEFAULT 0,
    ngay_tao DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_BaiViet_DanhMuc FOREIGN KEY (ma_danh_muc) REFERENCES danh_muc(ma_danh_muc) ON DELETE CASCADE,
    CONSTRAINT FK_BaiViet_TacGia FOREIGN KEY (ma_tac_gia) REFERENCES nguoi_dung(ma_nguoi_dung) ON DELETE CASCADE
);
GO

-- 4. BẢNG su_kien (Sự kiện / Hội thảo)
CREATE TABLE su_kien (
    ma_su_kien INT IDENTITY(1,1) PRIMARY KEY,
    ma_nguoi_tao INT NOT NULL,
    ten_su_kien NVARCHAR(255) NOT NULL,
    mo_ta NVARCHAR(MAX) NULL,
    dia_diem NVARCHAR(255) NOT NULL,
    thoi_gian_to_chuc DATETIME NOT NULL,
    han_dang_ky DATETIME NOT NULL,
    so_luong_toi_da INT NOT NULL DEFAULT 100,
    banner NVARCHAR(255) NULL,
    ngay_tao DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SuKien_NguoiTao FOREIGN KEY (ma_nguoi_tao) REFERENCES nguoi_dung(ma_nguoi_dung)
);
GO

-- 5. BẢNG dang_ky_su_kien (Hội viên đăng ký tham gia sự kiện)
CREATE TABLE dang_ky_su_kien (
    ma_dang_ky INT IDENTITY(1,1) PRIMARY KEY,
    ma_su_kien INT NOT NULL,
    ma_nguoi_dung INT NOT NULL,
    trang_thai VARCHAR(20) NOT NULL DEFAULT 'da_dang_ky',  -- 'da_dang_ky', 'da_huy', 'da_diem_danh'
    ghi_chu NVARCHAR(500) NULL,
    ngay_dang_ky DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_DKSK_SuKien FOREIGN KEY (ma_su_kien) REFERENCES su_kien(ma_su_kien) ON DELETE CASCADE,
    CONSTRAINT FK_DKSK_NguoiDung FOREIGN KEY (ma_nguoi_dung) REFERENCES nguoi_dung(ma_nguoi_dung) ON DELETE CASCADE,
    CONSTRAINT UQ_DKSK_NguoiDung_SuKien UNIQUE (ma_su_kien, ma_nguoi_dung)
);
GO

-- =============================================
-- THÊM DỮ LIỆU MẪU (SEED DATA)
-- Mật khẩu mặc định của 3 tài khoản là: 123456
-- Chuỗi hash BCrypt chuẩn bên dưới tương ứng với '123456'
-- =============================================

-- 1. Thêm dữ liệu mẫu vào bảng nguoi_dung
INSERT INTO nguoi_dung (ho_ten, email, so_dien_thoai, don_vi_cong_tac, chuc_danh, linh_vuc, mat_khau_ma_hoa, ngay_sinh, gioi_thieu, vai_tro, trang_thai) 
VALUES
 (N'Quản trị viên', 'admin@hiephoi.vn', '0900000001', N'Ban chấp hành', N'Quản trị hệ thống', NULL, '$2a$11$wE53O373tqP8z3vW.YmNMeiH7kKzE7Jm75dZJ/0lVfH3s9c2h6n5y', '1985-05-20', N'Tài khoản quản trị hệ thống.', 'quan_tri', 'hoat_dong'),
 (N'Trần Thị Mai', 'mai.tran@example.com', '0900000002', N'Công ty ABC', N'Trưởng phòng', N'Công nghệ thông tin', '$2a$11$wE53O373tqP8z3vW.YmNMeiH7kKzE7Jm75dZJ/0lVfH3s9c2h6n5y', '1990-03-12', N'Mong muốn kết nối với cộng đồng nghề nghiệp.', 'hoi_vien', 'hoat_dong'),
 (N'Phạm Văn Nam', 'nam.pham@example.com', '0900000003', N'XYZ', N'Chuyên viên', N'Marketing', '$2a$11$wE53O373tqP8z3vW.YmNMeiH7kKzE7Jm75dZJ/0lVfH3s9c2h6n5y', '1995-11-02', N'Muốn tham gia để học hỏi kinh nghiệm.', 'hoi_vien', 'cho_duyet');
GO

-- 2. Thêm dữ liệu mẫu vào bảng danh_muc (Danh mục bài viết)
INSERT INTO danh_muc (ten_danh_muc, mo_ta) 
VALUES
 (N'Tin hoạt động', N'Tin tức về các hoạt động của Hiệp hội'),
 (N'Thông báo', N'Thông báo chính thức từ Ban chấp hành');
GO

-- 3. Thêm dữ liệu mẫu vào bảng bai_viet (Tin tức)
INSERT INTO bai_viet (ma_danh_muc, ma_tac_gia, tieu_de, tom_tat, noi_dung, trang_thai) 
VALUES
 (1, 1, N'Khai mạc Hội thảo chuyên ngành 2026', N'Tóm tắt ngắn về hội thảo chuyên ngành công nghệ thông tin năm 2026.', N'Nội dung chi tiết bài viết khai mạc hội thảo...', 'da_dang'),
 (2, 1, N'Thông báo tuyển hội viên mới', N'Hiệp hội mở đăng ký hội viên đợt mới trong năm 2026.', N'Nội dung chi tiết thông báo tuyển hội viên...', 'da_dang');
GO

-- 4. Thêm dữ liệu mẫu vào bảng su_kien (Sự kiện / Hội thảo)
INSERT INTO su_kien (ma_nguoi_tao, ten_su_kien, mo_ta, dia_diem, thoi_gian_to_chuc, han_dang_ky, so_luong_toi_da) 
VALUES
 (1, N'Hội thảo chuyên ngành 2026 - Hà Nội', N'Hội thảo thường niên của Hiệp hội quy tụ các chuyên gia.', N'Trung tâm Hội nghị, Hà Nội', '2026-12-10 08:00:00', '2026-12-05 23:59:59', 100);
GO

-- 5. Thêm dữ liệu mẫu vào bảng dang_ky_su_kien (Đăng ký sự kiện của hội viên)
INSERT INTO dang_ky_su_kien (ma_su_kien, ma_nguoi_dung, trang_thai, ghi_chu) 
VALUES
 (1, 2, 'da_dang_ky', N'Mong ban tổ chức gửi tài liệu trước hội thảo.');
GO
