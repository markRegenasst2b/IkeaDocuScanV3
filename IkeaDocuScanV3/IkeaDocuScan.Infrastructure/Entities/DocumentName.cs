using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("DocumentName")]
public partial class DocumentName
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Name { get; set; }

    public int? DocumentTypeId { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("DocumentNames")]
    public virtual DocumentType? DocumentType { get; set; }

    [InverseProperty("DocumentName")]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
