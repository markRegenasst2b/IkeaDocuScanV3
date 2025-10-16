using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

public partial class UserPermission
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string AccountName { get; set; } = null!;

    public int? DocumentTypeId { get; set; }

    public int? CounterPartyId { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? CountryCode { get; set; }

    [ForeignKey("CounterPartyId")]
    [InverseProperty("UserPermissions")]
    public virtual CounterParty? CounterParty { get; set; }

    [ForeignKey("CountryCode")]
    [InverseProperty("UserPermissions")]
    public virtual Country? CountryCodeNavigation { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("UserPermissions")]
    public virtual DocumentType? DocumentType { get; set; }
}
