using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WNC15.Models
{
    [Table("bai_viet")]
    public class BaiViet
    {
        [Key]
        [Column("ma_bai_viet")]
        public int MaBaiViet { get; set; }

        [Required]
        [Column("ma_danh_muc")]
        public int MaDanhMuc { get; set; }

        [Required]
        [Column("ma_tac_gia")]
        public int MaTacGia { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Column("tieu_de")]
        [StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        [Column("tom_tat")]
        [StringLength(500)]
        public string? TomTat { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Column("noi_dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Column("anh_dai_dien")]
        [StringLength(255)]
        public string? AnhDaiDien { get; set; }

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "da_dang"; // "ban_nhap", "da_dang", "an"

        [Column("luot_xem")]
        public int LuotXem { get; set; } = 0;

        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(MaDanhMuc))]
        public virtual DanhMuc? DanhMuc { get; set; }

        [ForeignKey(nameof(MaTacGia))]
        public virtual NguoiDung? TacGia { get; set; }
    }
}
