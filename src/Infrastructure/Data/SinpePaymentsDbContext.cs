using sinpe_validator_api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SinpePaymentsDbContext : DbContext
{
    public SinpePaymentsDbContext(DbContextOptions<SinpePaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<ReceivedSms> ReceivedSms { get; set; }
    public DbSet<OrderPayment> OrderPayments { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidades catálogo
        ConfigureOrderStatus(modelBuilder);
        ConfigurePaymentStatus(modelBuilder);

        // Order Statuses Seed
        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { IdStatus = 1, Name = "Pending" },
            new OrderStatus { IdStatus = 2, Name = "Paid" },
            new OrderStatus { IdStatus = 3, Name = "Expired" },
            new OrderStatus { IdStatus = 4, Name = "UnderReview" }
        );

        // Payment Statuses Seed
        modelBuilder.Entity<PaymentStatus>().HasData(
            new PaymentStatus { IdStatus = 1, Name = "Approved" },
            new PaymentStatus { IdStatus = 2, Name = "Rejected" },
            new PaymentStatus { IdStatus = 3, Name = "UnderReview" }
        );

        // Configurations
        ConfigureOrder(modelBuilder);
        ConfigureReceivedSms(modelBuilder);
        ConfigureOrderPayment(modelBuilder);
    }

    private static void ConfigureOrderStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.IdStatus);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });
    }

    private static void ConfigurePaymentStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.IdStatus);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(e => e.IdStatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReceivedSms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReceivedSms>(entity =>
        {
            entity.HasKey(e => e.IdSms);
            entity.Property(e => e.SenderName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.SinpeReference).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RegisteredAt).HasDefaultValueSql("GETDATE()");
        });
    }

    private static void ConfigureOrderPayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderPayment>(entity =>
        {
            entity.HasKey(e => e.IdOrderPayment);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.ProcessedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.IdOrder)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Sms)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.IdSms)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.IdStatus)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
