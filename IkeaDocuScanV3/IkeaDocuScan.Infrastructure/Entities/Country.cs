using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("Country")]
public partial class Country
{
    [Key]
    [StringLength(2)]
    [Unicode(false)]
    public string CountryCode { get; set; } = null!;

    [StringLength(128)]
    [Unicode(false)]
    public string? Name { get; set; }

    [InverseProperty("CountryNavigation")]
    public virtual ICollection<CounterParty> CounterParties { get; set; } = new List<CounterParty>();

    [InverseProperty("CountryCodeNavigation")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
