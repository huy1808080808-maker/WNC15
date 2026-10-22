using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WNC15.Models
{
    [Table("danh_muc")]
    public class DanhMuc
    {
        [Key]
        [Column("ma_danh_muc")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [Column("ten_danh_muc")]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [Column("mo_ta")]
        [StringLength(255)]
        public string? MoTa { get; set; }

        // Navigation property
        public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();
    }
}
