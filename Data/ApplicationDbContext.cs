using Microsoft.EntityFrameworkCore;
using WNC15.Models;

namespace WNC15.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public DbSet<DanhMuc> DanhMucs { get; set; } = null!;
        public DbSet<BaiViet> BaiViets { get; set; } = null!;
        public DbSet<SuKien> SuKiens { get; set; } = null!;
        public DbSet<DangKySuKien> DangKySuKiens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Unique email cho NguoiDung
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình Unique (MaSuKien, MaNguoiDung) cho DangKySuKien
            modelBuilder.Entity<DangKySuKien>()
                .HasIndex(d => new { d.MaSuKien, d.MaNguoiDung })
                .IsUnique();

            // Quan hệ BaiViet - DanhMuc
            modelBuilder.Entity<BaiViet>()
                .HasOne(b => b.DanhMuc)
                .WithMany(d => d.BaiViets)
                .HasForeignKey(b => b.MaDanhMuc)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ BaiViet - TacGia (NguoiDung)
            modelBuilder.Entity<BaiViet>()
                .HasOne(b => b.TacGia)
                .WithMany(u => u.BaiViets)
                .HasForeignKey(b => b.MaTacGia)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ SuKien - NguoiTao (NguoiDung)
            modelBuilder.Entity<SuKien>()
                .HasOne(s => s.NguoiTao)
                .WithMany(u => u.SuKiens)
                .HasForeignKey(s => s.MaNguoiTao)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ DangKySuKien - SuKien
            modelBuilder.Entity<DangKySuKien>()
                .HasOne(d => d.SuKien)
                .WithMany(s => s.DangKySuKiens)
                .HasForeignKey(d => d.MaSuKien)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ DangKySuKien - NguoiDung
            modelBuilder.Entity<DangKySuKien>()
                .HasOne(d => d.NguoiDung)
                .WithMany(u => u.DangKySuKiens)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
