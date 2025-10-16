using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("CounterParty")]
[Index("CounterPartyNoAlpha", Name = "COUNTERPARTY_NOA_IDX")]
[Index("Since", Name = "COUNTERPARTY_SNC_IDX")]
public partial class CounterParty
{
    [Key]
    public int CounterPartyId { get; set; }

    public int CounterPartyNo { get; set; }

    [StringLength(128)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Since { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Address { get; set; }

    public bool DisplayAtCheckIn { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Comments { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string Country { get; set; } = null!;

    [StringLength(128)]
    [Unicode(false)]
    public string City { get; set; } = null!;

    [StringLength(128)]
    [Unicode(false)]
    public string? AffiliatedTo { get; set; }

    [StringLength(32)]
    [Unicode(false)]
    public string? CounterPartyNoAlpha { get; set; }

    [ForeignKey("Country")]
    [InverseProperty("CounterParties")]
    public virtual Country CountryNavigation { get; set; } = null!;

    [InverseProperty("CounterParty")]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    [InverseProperty("CounterParty")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
