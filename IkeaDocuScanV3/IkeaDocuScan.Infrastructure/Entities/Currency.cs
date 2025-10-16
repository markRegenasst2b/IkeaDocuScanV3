using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("Currency")]
public partial class Currency
{
    [Key]
    [StringLength(3)]
    [Unicode(false)]
    public string CurrencyCode { get; set; } = null!;

    [StringLength(128)]
    [Unicode(false)]
    public string? Name { get; set; }

    public int DecimalPlaces { get; set; }

    [InverseProperty("CurrencyCodeNavigation")]
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
