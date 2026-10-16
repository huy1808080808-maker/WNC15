using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WNC15.Models
{
    [Table("su_kien")]
    public class SuKien
    {
        [Key]
        [Column("ma_su_kien")]
        public int MaSuKien { get; set; }

        [Required]
        [Column("ma_nguoi_tao")]
        public int MaNguoiTao { get; set; }

        [Required(ErrorMessage = "Tên sự kiện không được để trống")]
        [Column("ten_su_kien")]
        [StringLength(255)]
        public string TenSuKien { get; set; } = string.Empty;

        [Column("mo_ta")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Địa điểm tổ chức không được để trống")]
        [Column("dia_diem")]
        [StringLength(255)]
        public string DiaDiem { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian tổ chức không được để trống")]
        [Column("thoi_gian_to_chuc")]
        public DateTime ThoiGianToChuc { get; set; }

        [Required(ErrorMessage = "Hạn đăng ký không được để trống")]
        [Column("han_dang_ky")]
        public DateTime HanDangKy { get; set; }

        [Column("so_luong_toi_da")]
        public int SoLuongToiDa { get; set; } = 100;

        [Column("banner")]
        [StringLength(255)]
        public string? Banner { get; set; }

        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(MaNguoiTao))]
        public virtual NguoiDung? NguoiTao { get; set; }

        public virtual ICollection<DangKySuKien> DangKySuKiens { get; set; } = new List<DangKySuKien>();
    }
}
