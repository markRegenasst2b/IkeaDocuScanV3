using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("Document")]
[Index("BarCode", Name = "uk_document_barcode", IsUnique = true)]
public partial class Document
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    public int BarCode { get; set; }

    [Column("DT_ID")]
    public int? DtId { get; set; }

    public int? CounterPartyId { get; set; }

    public int? DocumentNameId { get; set; }

    public int? FileId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DateOfContract { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Comment { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReceivingDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DispatchDate { get; set; }

    public bool? Fax { get; set; }

    public bool? OriginalReceived { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ActionDate { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ActionDescription { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ReminderGroup { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? DocumentNo { get; set; }

    [Column("AssociatedToPUA")]
    [StringLength(255)]
    [Unicode(false)]
    public string? AssociatedToPua { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? VersionNo { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? AssociatedToAppendix { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ValidUntil { get; set; }

    [StringLength(3)]
    [Unicode(false)]
    public string? CurrencyCode { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? Amount { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Authorisation { get; set; }

    public bool? BankConfirmation { get; set; }

    public bool? TranslatedVersionReceived { get; set; }

    public bool? Confidential { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string CreatedBy { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ModifiedBy { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ThirdParty { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? ThirdPartyId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SendingOutDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ForwardedToSignatoriesDate { get; set; }

    [ForeignKey("CounterPartyId")]
    [InverseProperty("Documents")]
    public virtual CounterParty? CounterParty { get; set; }

    [ForeignKey("CurrencyCode")]
    [InverseProperty("Documents")]
    public virtual Currency? CurrencyCodeNavigation { get; set; }

    [ForeignKey("DocumentNameId")]
    [InverseProperty("Documents")]
    public virtual DocumentName? DocumentName { get; set; }

    [ForeignKey("DtId")]
    [InverseProperty("Documents")]
    public virtual DocumentType? Dt { get; set; }

    [ForeignKey("FileId")]
    [InverseProperty("Documents")]
    public virtual DocumentFile? File { get; set; }
}
