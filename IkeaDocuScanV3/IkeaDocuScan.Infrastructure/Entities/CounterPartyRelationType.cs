using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("CounterPartyRelationType")]
public partial class CounterPartyRelationType
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Name { get; set; }

    [InverseProperty("RelationTypeNavigation")]
    public virtual ICollection<CounterPartyRelation> CounterPartyRelations { get; set; } = new List<CounterPartyRelation>();
}
