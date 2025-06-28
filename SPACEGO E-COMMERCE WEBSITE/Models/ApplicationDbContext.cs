using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Gọi base để không mất cấu hình Identity

            modelBuilder.Entity<DetailCartItem>()
                .HasKey(d => new { d.CartItemId, d.ProductId }); // Cấu hình khóa chính kết hợp
            modelBuilder.Entity<OrderProduct>()
                .HasKey(d => new { d.OrderId, d.ProductId }); // Cấu hình khóa chính kết hợp
                                                              // ⚠️ Ngăn cascade delete gây lỗi
            modelBuilder.Entity<DetailCartItem>()
            .HasKey(x => x.Id);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        //Biến thể sản phẩm
        public DbSet<Capacity> Capacities { get; set; }
        //Màu sắc sản phẩm
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<DetailCartItem> DetailsCartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        //bảng lưu trữ hoạt động người dùng
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }

    }
}
