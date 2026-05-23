using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sales.API.Domain.Entities;

namespace Sales.API.Infrastructure.Persistence;

public partial class SalesDbContext : DbContext
{
    public SalesDbContext()
    {
    }

    public SalesDbContext(DbContextOptions<SalesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<RestaurantOrder> RestaurantOrders { get; set; }

    public virtual DbSet<RestaurantOrderDetail> RestaurantOrderDetails { get; set; }

    public virtual DbSet<RestaurantOrderDetailStatus> RestaurantOrderDetailStatuses { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleDetail> SaleDetails { get; set; }

    public virtual DbSet<TaxConfiguration> TaxConfigurations { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamConfiguration> TeamConfigurations { get; set; }

    public virtual DbSet<Waiter> Waiters { get; set; }

    public virtual DbSet<WarehouseConfiguration> WarehouseConfigurations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=unifiedDB;Username=postgres;Password=Passw0rd123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("customers_pkey");

            entity.ToTable("customers", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.OrderDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.TaxPrice).HasPrecision(12, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_CustomerId_fkey");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_OrderStatusId_fkey");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_details_pkey");

            entity.ToTable("order_details", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ProductPrice).HasPrecision(12, 2);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_details_OrderId_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_statuses_pkey");

            entity.ToTable("order_statuses", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_types_pkey");

            entity.ToTable("payment_types", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RestaurantOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_orders_pkey");

            entity.ToTable("restaurant_orders", "sal");

            entity.HasIndex(e => e.Cen, "restaurant_orders_Cen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Order).WithMany(p => p.RestaurantOrders)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("restaurant_orders_OrderId_fkey");

            entity.HasOne(d => d.Waiter).WithMany(p => p.RestaurantOrders)
                .HasForeignKey(d => d.WaiterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("restaurant_orders_WaiterId_fkey");
        });

        modelBuilder.Entity<RestaurantOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_order_details_pkey");

            entity.ToTable("restaurant_order_details", "sal");

            entity.HasIndex(e => e.Cen, "restaurant_order_details_Cen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.SentAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.RestaurantOrderDetailStatus).WithMany(p => p.RestaurantOrderDetails)
                .HasForeignKey(d => d.RestaurantOrderDetailStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("restaurant_order_details_RestaurantOrderDetailStatusId_fkey");

            entity.HasOne(d => d.RestaurantOrder).WithMany(p => p.RestaurantOrderDetails)
                .HasForeignKey(d => d.RestaurantOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("restaurant_order_details_RestaurantOrderId_fkey");
        });

        modelBuilder.Entity<RestaurantOrderDetailStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_order_detail_statuses_pkey");

            entity.ToTable("restaurant_order_detail_statuses", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_pkey");

            entity.ToTable("sales", "sal");

            entity.HasIndex(e => e.Cen, "sales_Cen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.SaleDatetime)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SubtotalPrice).HasPrecision(12, 2);
            entity.Property(e => e.TaxPrice).HasPrecision(12, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_CustomerId_fkey");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Sales)
                .HasForeignKey(d => d.PaymentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_PaymentTypeId_fkey");
        });

        modelBuilder.Entity<SaleDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sale_details_pkey");

            entity.ToTable("sale_details", "sal");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Price).HasPrecision(12, 2);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Sale).WithMany(p => p.SaleDetails)
                .HasForeignKey(d => d.SaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_details_SaleId_fkey");
        });

        modelBuilder.Entity<TaxConfiguration>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("tax_configurations_pkey");

            entity.ToTable("tax_configurations", "sal");

            entity.HasIndex(e => e.CompanyCen, "tax_configurations_CompanyCen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.GlobalTaxPercentage).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teams_pkey");

            entity.ToTable("teams", "sal");

            entity.HasIndex(e => e.Cen, "teams_Cen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<TeamConfiguration>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.CategoryId, e.TeamId }).HasName("team_configurations_pkey");

            entity.ToTable("team_configurations", "sal");

            entity.HasOne(d => d.Team).WithMany(p => p.TeamConfigurations)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("team_configurations_TeamId_fkey");
        });

        modelBuilder.Entity<Waiter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("waiters_pkey");

            entity.ToTable("waiters", "sal");

            entity.HasIndex(e => e.Cen, "waiters_Cen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<WarehouseConfiguration>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("warehouse_configurations_pkey");

            entity.ToTable("warehouse_configurations", "sal");

            entity.HasIndex(e => e.CompanyCen, "warehouse_configurations_CompanyCen_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
