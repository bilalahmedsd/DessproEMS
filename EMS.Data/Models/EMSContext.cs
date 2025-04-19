using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EMS.Data.Models;

public partial class EMSContext : DbContext
{
    public EMSContext()
    {
    }

    public EMSContext(DbContextOptions<EMSContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceDataDetail> DeviceDataDetails { get; set; }

    public virtual DbSet<DeviceDataMaster> DeviceDataMasters { get; set; }

    public virtual DbSet<DeviceRawDatum> DeviceRawData { get; set; }

    public virtual DbSet<Gateway> Gateways { get; set; }

    public virtual DbSet<ProjectManagement> ProjectManagements { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=192.168.15.60,1433;Initial Catalog=EMS;User ID=sa;Password=sql;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Company__3214EC072B965546");

            entity.ToTable("Company");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Devices__3214EC07D82AD161");

            entity.Property(e => e.ChannelName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ConsumptionUnit)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.OfflineDuration)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.OfflineTime).HasColumnType("datetime");
            entity.Property(e => e.SerialNo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SerialPort)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<DeviceDataDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceDa__3214EC075D6E8397");

            entity.Property(e => e.Address)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });
        modelBuilder.Entity<DeviceDataDetail>()
    .HasOne(detail => detail.DeviceDataMaster)
    .WithMany(master => master.DeviceDataDetails)
    .HasForeignKey(detail => detail.FkDeviceDataMasterId)
    .OnDelete(DeleteBehavior.NoAction); // optional: prevents cascade delete errors

        modelBuilder.Entity<DeviceDataMaster>()
            .HasOne(master => master.Device)
            .WithMany(device => device.DeviceDataMasters)
            .HasForeignKey(master => master.FkDeviceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Device>()
            .HasOne(device => device.Unit)
            .WithMany(unit => unit.Devices)
            .HasForeignKey(device => device.FkUnitId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Unit>()
            .HasOne(unit => unit.ProjectManagement)
            .WithMany(project => project.Units)
            .HasForeignKey(unit => unit.FkProjectManagement)
            .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<DeviceDataMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceDa__3214EC070F75541A");

            entity.ToTable("DeviceDataMaster");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DeviceDataMaster>()
    .HasOne(d => d.Device)
    .WithMany(p => p.DeviceDataMasters)
    .HasForeignKey(d => d.FkDeviceId)
    .OnDelete(DeleteBehavior.ClientSetNull); // Or your preferred behavior

        modelBuilder.Entity<DeviceDataDetail>()
            .HasOne(d => d.DeviceDataMaster)
            .WithMany(p => p.DeviceDataDetails)
            .HasForeignKey(d => d.FkDeviceDataMasterId)
            .OnDelete(DeleteBehavior.ClientSetNull);


        modelBuilder.Entity<DeviceRawDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceRa__3214EC073E071B9E");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Data)
                .HasColumnType("text")
                .HasColumnName("data");
        });

        modelBuilder.Entity<Gateway>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Gateway__3214EC075C5A8F3A");

            entity.ToTable("Gateway");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProtocolName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SerialNo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProjectManagement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProjectM__3214EC074B81EDD7");

            entity.ToTable("ProjectManagement");

            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Principal)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Unit__3214EC077B596CE0");

            entity.ToTable("Unit");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07224B1DC3");

            entity.ToTable("User");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserType__3214EC0718467D28");

            entity.ToTable("UserType");

            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
