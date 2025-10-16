using System;
using System.Collections.Generic;
using IkeaDocuScan.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditTrail> AuditTrails { get; set; }

    public virtual DbSet<CounterParty> CounterParties { get; set; }

    public virtual DbSet<CounterPartyRelation> CounterPartyRelations { get; set; }

    public virtual DbSet<CounterPartyRelationType> CounterPartyRelationTypes { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentFile> DocumentFiles { get; set; }

    public virtual DbSet<DocumentName> DocumentNames { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;initial catalog=IkeaDocumentScanningCH;integrated security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditTrail__4AB81AF0");
        });

        modelBuilder.Entity<CounterParty>(entity =>
        {
            entity.HasKey(e => e.CounterPartyId).HasName("PK__CounterParty__3B75D760");

            entity.Property(e => e.CounterPartyNo).HasDefaultValue(-1);
            entity.Property(e => e.CounterPartyNoAlpha).HasDefaultValue("");
            entity.Property(e => e.Country).IsFixedLength();
            entity.Property(e => e.DisplayAtCheckIn).HasDefaultValue(true);

            entity.HasOne(d => d.CountryNavigation).WithMany(p => p.CounterParties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CounterPa__Count__3D5E1FD2");
        });

        modelBuilder.Entity<CounterPartyRelation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CounterP__3214EC274F1EBB10");

            entity.HasOne(d => d.RelationTypeNavigation).WithMany(p => p.CounterPartyRelations).HasConstraintName("FK__CounterPa__Relat__5FB337D6");
        });

        modelBuilder.Entity<CounterPartyRelationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CounterP__3214EC27E77D97BE");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryCode).HasName("PK__Country__5D9B0D2D34531A97");

            entity.Property(e => e.CountryCode).IsFixedLength();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyCode).HasName("PK__Currency__408426BEA95ECF99");

            entity.Property(e => e.CurrencyCode).IsFixedLength();
            entity.Property(e => e.DecimalPlaces).HasDefaultValue(2);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DOCUMENT_PK");

            entity.Property(e => e.CurrencyCode).IsFixedLength();

            entity.HasOne(d => d.CounterParty).WithMany(p => p.Documents).HasConstraintName("FK__Document__Counte__5EBF139D");

            entity.HasOne(d => d.CurrencyCodeNavigation).WithMany(p => p.Documents).HasConstraintName("FK__Document__Curren__619B8048");

            entity.HasOne(d => d.DocumentName).WithMany(p => p.Documents).HasConstraintName("FK__Document__Docume__5FB337D6");

            entity.HasOne(d => d.Dt).WithMany(p => p.Documents).HasConstraintName("FK__Document__DT_ID__6383C8BA");

            entity.HasOne(d => d.File).WithMany(p => p.Documents).HasConstraintName("FK__Document__FileId__5AEE82B9");
        });

        modelBuilder.Entity<DocumentFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DOCUMENTFILE_PK");

            entity.Property(e => e.FileType).HasDefaultValue(".pdf");
        });

        modelBuilder.Entity<DocumentName>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DocumentName__47DBAE45");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.DocumentNames).HasConstraintName("FK__DocumentN__Docum__48CFD27E");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.DtId).HasName("PK__Document__148CEA33753529D2");

            entity.Property(e => e.DtId).ValueGeneratedNever();
            entity.Property(e => e.ActionDate)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.ActionDescription)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.Amount)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.AssociatedToAppendix)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.AssociatedToPua)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.Authorisation)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.BankConfirmation)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.BarCode)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.Comment)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.Confidential)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.CounterParty)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.CounterPartyAlpha)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.Currency)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.DateOfContract)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.DispatchDate)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.DocumentNo)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.Fax)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.ForwardedToSignatoriesDate)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.OriginalReceived)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.ReceivingDate)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.ReminderGroup)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.SendingOutDate)
                .HasDefaultValue("O")
                .IsFixedLength();
            entity.Property(e => e.TranslatedVersionReceived)
                .HasDefaultValue("M")
                .IsFixedLength();
            entity.Property(e => e.ValidUntil)
                .HasDefaultValue("N")
                .IsFixedLength();
            entity.Property(e => e.VersionNo)
                .HasDefaultValue("N")
                .IsFixedLength();
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("USERACCOUNT_PK");

            entity.Property(e => e.CountryCode).IsFixedLength();

            entity.HasOne(d => d.CounterParty).WithMany(p => p.UserPermissions).HasConstraintName("FK__UserPermi__Count__6A30C649");

            entity.HasOne(d => d.CountryCodeNavigation).WithMany(p => p.UserPermissions).HasConstraintName("FK__UserPermi__Count__693CA210");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.UserPermissions).HasConstraintName("FK__UserPermi__Docum__6A30C649");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
