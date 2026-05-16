using Microsoft.EntityFrameworkCore;
using Sales.API.Domain.Entities;

namespace Sales.API.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {
    }

    // Entidades de Lectura (Esquema Inventory)
    public DbSet<Company> Companies { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductStock> ProductStocks { get; set; }
    public DbSet<WarehouseRef> Warehouses { get; set; }

    // Entidades Propias (Esquema Sales)
    public DbSet<SalesSetting> SalesSettings { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketItem> TicketItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("locations", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Active).HasColumnName("active");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Sku).HasColumnName("sku");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.IsOutOfStock).HasColumnName("is_out_of_stock");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.StationCode).HasColumnName("station_code");

            entity.HasOne(d => d.Category).WithMany().HasForeignKey(d => d.CategoryId);
        });

        modelBuilder.Entity<WarehouseRef>(entity =>
        {
            entity.ToTable("warehouses", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Code).HasColumnName("code");
        });

        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.ToTable("stocks", "inventory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Product).WithMany(p => p.Stocks).HasForeignKey(d => d.ProductId);
            entity.HasOne(d => d.Warehouse).WithMany().HasForeignKey(d => d.WarehouseId);
        });

        // --- MAPEO INTERNO (SALES) ---
        modelBuilder.Entity<SalesSetting>(entity =>
        {
            entity.ToTable("sales_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.TaxRate).HasColumnName("tax_rate");
            entity.Property(e => e.PaymentMethods).HasColumnName("payment_methods");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("vendors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.IsWaiter).HasColumnName("is_waiter");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("tickets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.TicketNumber).HasColumnName("ticket_number");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ServiceType).HasColumnName("service_type");
            entity.Property(e => e.TableCode).HasColumnName("table_code");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.TaxRate).HasColumnName("tax_rate");
            entity.Property(e => e.TaxAmount).HasColumnName("tax_amount");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.OpenedAt).HasColumnName("opened_at");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId);
            entity.HasOne(d => d.Location).WithMany().HasForeignKey(d => d.LocationId);
            entity.HasOne(d => d.Vendor).WithMany(p => p.Tickets).HasForeignKey(d => d.VendorId);
            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets).HasForeignKey(d => d.CustomerId);
        });

        modelBuilder.Entity<TicketItem>(entity =>
        {
            entity.ToTable("ticket_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            
            // Subtotal calculado y almacenado por BD
            entity.Property(e => e.Subtotal)
                  .HasColumnName("subtotal")
                  .HasComputedColumnSql("(ROUND(quantity * unit_price, 2))", stored: true);
                  
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Items).HasForeignKey(d => d.TicketId);
            entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Reference).HasColumnName("reference");
            entity.Property(e => e.PaidBy).HasColumnName("paid_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Payments).HasForeignKey(d => d.TicketId);
        });
    }
}