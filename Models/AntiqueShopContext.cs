using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AntiqueShopMVC.Models;

public partial class AntiqueShopContext : DbContext
{
    public AntiqueShopContext()
    {
    }

    public AntiqueShopContext(DbContextOptions<AntiqueShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemLog> ItemLogs { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<OriginCountry> OriginCountries { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AntiqueShop;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.IdCart).HasName("PK__Cart__C71FE317337FD7CA");

            entity.ToTable("Cart");

            entity.Property(e => e.IdCart).HasColumnName("id_cart");
            entity.Property(e => e.AddedDate).HasColumnName("added_date");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK__Cart__id_item__3B75D760");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK__Cart__id_user__3C69FB99");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PK__Categori__E548B673DC1F1223");

            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.NameCategory)
                .HasMaxLength(100)
                .HasColumnName("name_category");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.IdFavorite).HasName("PK__Favorite__78F875A4FF1F6022");

            entity.Property(e => e.IdFavorite).HasColumnName("id_favorite");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK__Favorites__id_it__3D5E1FD2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK__Favorites__id_us__3E52440B");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PK__Items__87C9438BB80219DC");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.ArrivalDate).HasColumnName("arrival_date");
            entity.Property(e => e.Authenticity)
                .HasMaxLength(50)
                .HasColumnName("authenticity");
            entity.Property(e => e.Condition)
                .HasMaxLength(50)
                .HasColumnName("condition");
            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.IdOriginCountry).HasColumnName("id_origin_country");
            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.NameItem)
                .HasMaxLength(100)
                .HasColumnName("name_item");
            entity.Property(e => e.PurchasePrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("purchase_price");
            entity.Property(e => e.SellingPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("selling_price");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdCategory)
                .HasConstraintName("FK__Items__id_catego__403A8C7D");

            entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdMaterial)
                .HasConstraintName("FK__Items__id_materi__412EB0B6");

            entity.HasOne(d => d.IdOriginCountryNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdOriginCountry)
                .HasConstraintName("FK__Items__id_origin__4222D4EF");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdStatus)
                .HasConstraintName("FK__Items__id_status__4316F928");

            entity.HasOne(d => d.IdSupplierNavigation).WithMany(p => p.Items)
                .HasForeignKey(d => d.IdSupplier)
                .HasConstraintName("FK__Items__id_suppli__440B1D61");
        });

        modelBuilder.Entity<ItemLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__Item_log__6CC851FE35B5CB2F");

            entity.ToTable("Item_logs");

            entity.Property(e => e.IdLog).HasColumnName("id_log");
            entity.Property(e => e.ChangeDate)
                .HasColumnType("datetime")
                .HasColumnName("change_date");
            entity.Property(e => e.ChangedField)
                .HasMaxLength(100)
                .HasColumnName("changed_field");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.NewValue)
                .HasMaxLength(255)
                .HasColumnName("new_value");
            entity.Property(e => e.OldValue)
                .HasMaxLength(255)
                .HasColumnName("old_value");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.ItemLogs)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK__Item_logs__id_it__3F466844");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.IdMaterial).HasName("PK__Material__81E99B839E52087A");

            entity.Property(e => e.IdMaterial).HasColumnName("id_material");
            entity.Property(e => e.NameMaterial)
                .HasMaxLength(100)
                .HasColumnName("name_material");
        });

        modelBuilder.Entity<OriginCountry>(entity =>
        {
            entity.HasKey(e => e.IdOriginCountry).HasName("PK__Origin_c__24703D4A9A182997");

            entity.ToTable("Origin_countries");

            entity.Property(e => e.IdOriginCountry).HasColumnName("id_origin_country");
            entity.Property(e => e.NameCountry)
                .HasMaxLength(100)
                .HasColumnName("name_country");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK__Role__3D48441D35510FFD");

            entity.ToTable("Role");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.NameRole)
                .HasMaxLength(50)
                .HasColumnName("name_role");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.IdSale).HasName("PK__Sales__D18B0157C94DD7DC");

            entity.Property(e => e.IdSale).HasColumnName("id_sale");
            entity.Property(e => e.FinalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("final_price");
            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.SaleDate).HasColumnName("sale_date");

            entity.HasOne(d => d.IdItemNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.IdItem)
                .HasConstraintName("FK__Sales__id_item__44FF419A");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK__Sales__id_user__45F365D3");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("PK__Statuses__5D2DC6E84558B2EE");

            entity.Property(e => e.IdStatus).HasColumnName("id_status");
            entity.Property(e => e.NameStatus)
                .HasMaxLength(50)
                .HasColumnName("name_status");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier).HasName("PK__Supplier__F6C576E6C29BCC41");

            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.Authenticity)
                .HasMaxLength(100)
                .HasColumnName("authenticity");
            entity.Property(e => e.NameSupplier)
                .HasMaxLength(100)
                .HasColumnName("name_supplier");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__Users__D2D1463729716EA2");

            entity.HasIndex(e => e.Login, "UQ__Users__7838F2721B088DF8").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .HasColumnName("middle_name");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Preferences)
                .HasMaxLength(255)
                .HasColumnName("preferences");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__id_role__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
