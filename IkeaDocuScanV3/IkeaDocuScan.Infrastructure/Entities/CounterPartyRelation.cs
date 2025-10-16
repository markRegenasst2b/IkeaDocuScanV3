using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("CounterPartyRelation")]
[Index("ChildCounterPartyNoAlpha", Name = "COUNTERPARTYRELATION_CHILD_IDX")]
[Index("ParentCounterPartyNoAlpha", Name = "COUNTERPARTYRELATION_PARENT_IDX")]
public partial class CounterPartyRelation
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? ParentCounterPartyNo { get; set; }

    public int? ChildCounterPartyNo { get; set; }

    public int? RelationType { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Comment { get; set; }

    [StringLength(32)]
    [Unicode(false)]
    public string? ParentCounterPartyNoAlpha { get; set; }

    [StringLength(32)]
    [Unicode(false)]
    public string? ChildCounterPartyNoAlpha { get; set; }

    [ForeignKey("RelationType")]
    [InverseProperty("CounterPartyRelations")]
    public virtual CounterPartyRelationType? RelationTypeNavigation { get; set; }
}
