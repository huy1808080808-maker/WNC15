using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WNC15.Models
{
    [Table("dang_ky_su_kien")]
    public class DangKySuKien
    {
        [Key]
        [Column("ma_dang_ky")]
        public int MaDangKy { get; set; }

        [Required]
        [Column("ma_su_kien")]
        public int MaSuKien { get; set; }

        [Required]
        [Column("ma_nguoi_dung")]
        public int MaNguoiDung { get; set; }

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "da_dang_ky"; // "da_dang_ky", "da_huy", "da_diem_danh"

        [Column("ghi_chu")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Column("ngay_dang_ky")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(MaSuKien))]
        public virtual SuKien? SuKien { get; set; }

        [ForeignKey(nameof(MaNguoiDung))]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
