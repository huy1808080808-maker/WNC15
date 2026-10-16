using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WNC15.Models
{
    [Table("nguoi_dung")]
    public class NguoiDung
    {
        [Key]
        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Column("ho_ten")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("so_dien_thoai")]
        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Column("don_vi_cong_tac")]
        [StringLength(200)]
        public string? DonViCongTac { get; set; }

        [Column("chuc_danh")]
        [StringLength(100)]
        public string? ChucDanh { get; set; }

        [Column("linh_vuc")]
        [StringLength(100)]
        public string? LinhVuc { get; set; }

        [Required]
        [Column("mat_khau_ma_hoa")]
        [StringLength(255)]
        public string MatKhauMaHoa { get; set; } = string.Empty;

        [Column("ngay_sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Column("gioi_thieu")]
        public string? GioiThieu { get; set; }

        [Column("anh_dai_dien")]
        [StringLength(255)]
        public string? AnhDaiDien { get; set; }

        [Column("vai_tro")]
        [StringLength(20)]
        public string VaiTro { get; set; } = "hoi_vien"; // "quan_tri", "hoi_vien"

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "cho_duyet"; // "cho_duyet", "hoat_dong", "tu_choi", "khoa"

        [Column("ly_do_tu_choi")]
        [StringLength(500)]
        public string? LyDoTuChoi { get; set; }

        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
        public virtual ICollection<SuKien> SuKiens { get; set; } = new List<SuKien>();
        public virtual ICollection<DangKySuKien> DangKySuKiens { get; set; } = new List<DangKySuKien>();
    }
}
