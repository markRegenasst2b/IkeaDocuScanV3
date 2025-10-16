using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("DocumentFile")]
[Index("FileName", Name = "uk_file_filename", IsUnique = true)]
public partial class DocumentFile
{
    [Key]
    public int Id { get; set; }

    [Unicode(false)]
    public string FileName { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string FileType { get; set; } = null!;

    public byte[]? Bytes { get; set; }

    [InverseProperty("File")]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
