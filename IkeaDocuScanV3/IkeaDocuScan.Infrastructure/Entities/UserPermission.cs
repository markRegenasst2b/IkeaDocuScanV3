using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Index("UserId", Name = "IX_UserPermissions_UserId")]
public partial class UserPermission
{
    [Key]
    public int Id { get; set; }

    public int? DocumentTypeId { get; set; }

    public int? CounterPartyId { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? CountryCode { get; set; }

    public int UserId { get; set; }

    [ForeignKey("CounterPartyId")]
    [InverseProperty("UserPermissions")]
    public virtual CounterParty? CounterParty { get; set; }

    [ForeignKey("CountryCode")]
    [InverseProperty("UserPermissions")]
    public virtual Country? CountryCodeNavigation { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("UserPermissions")]
    public virtual DocumentType? DocumentType { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserPermissions")]
    public virtual DocuScanUser User { get; set; } = null!;
}
