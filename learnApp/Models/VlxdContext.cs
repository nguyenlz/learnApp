using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using learnApp.Models;

namespace learnApp.Models;

public partial class VlxdContext : DbContext
{
    public VlxdContext()
    {
    }

    public VlxdContext(DbContextOptions<VlxdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<StockImport> StockImports { get; set; }

    public virtual DbSet<StockImportDetail> StockImportDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Site> Sites { get; set; } = default!;
    public DbSet<SupplierPayment> SupplierPayments { get; set; } = default!;
    public DbSet<CustomerPayment> CustomerPayments { get; set; } = default!;


    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=DESKTOP-B98ANRQ\\SQLEXPRESS;Database=VLXD;Trusted_Connection=True;TrustServerCertificate=true");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BB0D675F9");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E09337AAF7").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B89BDD4CFF");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D10534AA055A69").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CustomerName).HasMaxLength(150);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF1ACFBC283");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.EmployeeName).HasMaxLength(150);
            entity.Property(e => e.HireDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFC6915AF0");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.SiteId).HasColumnName("SiteID");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasPrecision(14, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(14, 2).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.Property(e => e.PaymentStatus)
                .HasConversion<int>();

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Customer");

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Employee");

            entity.HasOne(d => d.Site)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.SiteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Order_Site");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("PK__OrderDet__08D097C11AF87EFB");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasPrecision(14, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(14, 2);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6EDDA36C0E7");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.StockQuantity).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasDefaultValue("Bao");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Supplier");
        });

        modelBuilder.Entity<StockImport>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__StockImp__8697678A4251ADFD");

            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.ImportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DebtAmount).HasColumnType("decimal(18, 2)");


            entity.HasOne(d => d.Employee).WithMany(p => p.StockImports)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Import_Employee");

            entity.HasOne(d => d.Supplier).WithMany(p => p.StockImports)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Import_Supplier");
        });

        modelBuilder.Entity<StockImportDetail>(entity =>
        {
            entity.HasKey(e => new { e.ImportId, e.ProductId }).HasName("PK__StockImp__4DD7ABE466E9BCBD");

            entity.Property(e => e.ImportId).HasColumnName("ImportID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ImportPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Import).WithMany(p => p.StockImportDetails)
                .HasForeignKey(d => d.ImportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportDetail_Import");

            entity.HasOne(d => d.Product).WithMany(p => p.StockImportDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportDetail_Product");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE6669444475902");

            entity.HasIndex(e => e.Phone, "UQ__Supplier__5C7E359E78C592E2").IsUnique();

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName).HasMaxLength(150);
        });

        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.HasKey(e => e.SupplierPaymentId);

            entity.Property(e => e.SupplierPaymentId).HasColumnName("SupplierPaymentId");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("getdate()");

            entity.HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierPayments)
                .HasForeignKey(sp => sp.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerPayment>(entity =>
        {
            entity.HasKey(e => e.CustomerPaymentId);

            entity.Property(e => e.CustomerPaymentId).HasColumnName("CustomerPaymentId");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("getdate()");

            entity.HasOne(cp => cp.Customer)
                .WithMany(c => c.CustomerPayments)
                .HasForeignKey(cp => cp.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

public DbSet<learnApp.Models.CustomerPayment> CustomerPayment { get; set; } = default!;

}
