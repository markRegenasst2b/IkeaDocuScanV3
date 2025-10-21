using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("CounterPartyRelation")]
[Index("ChildCounterPartyId", Name = "IX_CounterPartyRelation_ChildCounterPartyId")]
[Index("ParentCounterPartyId", Name = "IX_CounterPartyRelation_ParentCounterPartyId")]
public partial class CounterPartyRelation
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? RelationType { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Comment { get; set; }

    public int? ParentCounterPartyId { get; set; }

    public int? ChildCounterPartyId { get; set; }

    [ForeignKey("ChildCounterPartyId")]
    [InverseProperty("CounterPartyRelationChildCounterParties")]
    public virtual CounterParty? ChildCounterParty { get; set; }

    [ForeignKey("ParentCounterPartyId")]
    [InverseProperty("CounterPartyRelationParentCounterParties")]
    public virtual CounterParty? ParentCounterParty { get; set; }

    [ForeignKey("RelationType")]
    [InverseProperty("CounterPartyRelations")]
    public virtual CounterPartyRelationType? RelationTypeNavigation { get; set; }
}
