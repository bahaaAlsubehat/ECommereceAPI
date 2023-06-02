using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Ref.Lect4.Models
{
    public partial class OnlineStoreContext : DbContext
    {
        public OnlineStoreContext()
        {
        }

        public OnlineStoreContext(DbContextOptions<OnlineStoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cart> Cart { get; set; }
        public virtual DbSet<CartItem> CartItem { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Items> Items { get; set; }
        public virtual DbSet<Login> Login { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderStatus> OrderStatus { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserType> UserType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=DESKTOP-46F1969;Database=OnlineStore;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Cart)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Cart_User");
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.Property(e => e.Note)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Cart)
                    .WithMany(p => p.CartItem)
                    .HasForeignKey(d => d.CartId)
                    .HasConstraintName("FK_CartItemId_Cart");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.CartItem)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("FK_CartItemId_Items");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Items>(entity =>
            {
                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.Description)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_Items_Category");
            });

            modelBuilder.Entity<Login>(entity =>
            {
                entity.Property(e => e.CurrentToken).IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Login)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Login_User");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.Note)
                    .HasMaxLength(500)
                    .IsFixedLength();

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.HasOne(d => d.ApprovedByNavigation)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.ApprovedBy)
                    .HasConstraintName("FK_Order_Cart");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK_Order_OrderStatus");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsFixedLength();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.UserType)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.UserTypeId)
                    .HasConstraintName("FK_Table_2_UserType");
            });

            modelBuilder.Entity<UserType>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
