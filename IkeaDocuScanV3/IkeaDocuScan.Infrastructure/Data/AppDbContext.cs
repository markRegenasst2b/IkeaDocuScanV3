using System;
using System.Collections.Generic;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Infrastructure.Entities.Configuration;
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

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<DocuScanUser> DocuScanUsers { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentFile> DocumentFiles { get; set; }

    public virtual DbSet<DocumentName> DocumentNames { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    // Configuration Management DbSets
    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public virtual DbSet<SystemConfigurationAudit> SystemConfigurationAudits { get; set; }
    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
    public virtual DbSet<EmailRecipientGroup> EmailRecipientGroups { get; set; }
    public virtual DbSet<EmailRecipient> EmailRecipients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditTrail__4AB81AF0");
        });

        modelBuilder.Entity<CounterParty>(entity =>
        {
            entity.HasKey(e => e.CounterPartyId).HasName("PK__CounterParty__3B75D760");

            entity.Property(e => e.CounterPartyNoAlpha).HasDefaultValue("");
            entity.Property(e => e.Country).IsFixedLength();
            entity.Property(e => e.DisplayAtCheckIn).HasDefaultValue(true);

            entity.HasOne(d => d.CountryNavigation).WithMany(p => p.CounterParties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CounterPa__Count__3D5E1FD2");
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

        modelBuilder.Entity<DocuScanUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("DOCUSCANUSER_PK");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
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

            entity.HasOne(d => d.User).WithMany(p => p.UserPermissions).HasConstraintName("FK_UserPermissions_DocuScanUser");
        });

        // Configure SystemConfiguration
        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.ToTable("SystemConfiguration");
            entity.HasKey(e => e.ConfigurationId);

            entity.Property(e => e.ConfigKey).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ConfigSection).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConfigValue).IsRequired();
            entity.Property(e => e.ValueType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);

            entity.HasIndex(e => e.ConfigKey).IsUnique().HasDatabaseName("IX_SystemConfiguration_ConfigKey");
            entity.HasIndex(e => new { e.ConfigSection, e.IsActive }).HasDatabaseName("IX_SystemConfiguration_Section_Active");

            entity.ToTable(t => t.HasCheckConstraint("CK_SystemConfiguration_Section",
                "ConfigSection IN ('Email', 'ActionReminderService', 'General', 'System')"));
        });

        // Configure SystemConfigurationAudit
        modelBuilder.Entity<SystemConfigurationAudit>(entity =>
        {
            entity.ToTable("SystemConfigurationAudit");
            entity.HasKey(e => e.AuditId);

            entity.Property(e => e.ConfigKey).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ChangedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ChangedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ChangeReason).HasMaxLength(500);

            entity.HasOne(e => e.Configuration)
                .WithMany(e => e.AuditTrail)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ConfigAudit_Config");

            entity.HasIndex(e => e.ConfigurationId).HasDatabaseName("IX_SystemConfigurationAudit_ConfigId");
            entity.HasIndex(e => e.ChangedDate).HasDatabaseName("IX_SystemConfigurationAudit_ChangedDate");
        });

        // Configure EmailTemplate
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("EmailTemplate");
            entity.HasKey(e => e.TemplateId);

            entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TemplateKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
            entity.Property(e => e.HtmlBody).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);

            entity.HasIndex(e => e.TemplateName).IsUnique().HasDatabaseName("IX_EmailTemplate_Name");
            entity.HasIndex(e => e.TemplateKey).IsUnique().HasDatabaseName("IX_EmailTemplate_Key");
            entity.HasIndex(e => new { e.TemplateKey, e.IsActive }).HasDatabaseName("IX_EmailTemplate_Key_Active");
        });

        // Configure EmailRecipientGroup
        modelBuilder.Entity<EmailRecipientGroup>(entity =>
        {
            entity.ToTable("EmailRecipientGroup");
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.GroupKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.GroupName).IsUnique().HasDatabaseName("IX_EmailRecipientGroup_Name");
            entity.HasIndex(e => e.GroupKey).IsUnique().HasDatabaseName("IX_EmailRecipientGroup_Key");
        });

        // Configure EmailRecipient
        modelBuilder.Entity<EmailRecipient>(entity =>
        {
            entity.ToTable("EmailRecipient");
            entity.HasKey(e => e.RecipientId);

            entity.Property(e => e.EmailAddress).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Group)
                .WithMany(e => e.Recipients)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EmailRecipient_Group");

            entity.HasIndex(e => new { e.GroupId, e.EmailAddress })
                .IsUnique()
                .HasDatabaseName("IX_EmailRecipient_Group_Email");
            entity.HasIndex(e => new { e.GroupId, e.IsActive }).HasDatabaseName("IX_EmailRecipient_Group_Active");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
