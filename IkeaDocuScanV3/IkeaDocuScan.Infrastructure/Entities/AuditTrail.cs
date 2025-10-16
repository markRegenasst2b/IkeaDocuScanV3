using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("AuditTrail")]
public partial class AuditTrail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [StringLength(128)]
    [Unicode(false)]
    public string User { get; set; } = null!;

    [StringLength(128)]
    [Unicode(false)]
    public string Action { get; set; } = null!;

    [StringLength(2500)]
    [Unicode(false)]
    public string? Details { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string BarCode { get; set; } = null!;
}
