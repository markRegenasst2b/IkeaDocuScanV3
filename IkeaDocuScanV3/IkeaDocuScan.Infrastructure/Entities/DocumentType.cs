using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("DocumentType")]
public partial class DocumentType
{
    [Key]
    [Column("DT_ID")]
    public int DtId { get; set; }

    [Column("DT_Name")]
    [StringLength(255)]
    [Unicode(false)]
    public string DtName { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string BarCode { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string CounterParty { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string DateOfContract { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Comment { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ReceivingDate { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string DispatchDate { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Fax { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string OriginalReceived { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string DocumentNo { get; set; } = null!;

    [Column("AssociatedToPUA")]
    [StringLength(1)]
    [Unicode(false)]
    public string AssociatedToPua { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string VersionNo { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string AssociatedToAppendix { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ValidUntil { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Amount { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Authorisation { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string BankConfirmation { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string TranslatedVersionReceived { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ActionDate { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ActionDescription { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ReminderGroup { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Confidential { get; set; } = null!;

    public bool IsAppendix { get; set; }

    public bool IsEnabled { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string CounterPartyAlpha { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string SendingOutDate { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string ForwardedToSignatoriesDate { get; set; } = null!;

    [InverseProperty("DocumentType")]
    public virtual ICollection<DocumentName> DocumentNames { get; set; } = new List<DocumentName>();

    [InverseProperty("Dt")]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    [InverseProperty("DocumentType")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
