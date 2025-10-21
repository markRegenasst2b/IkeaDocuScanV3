using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Entities;

[Table("DocuScanUser")]
[Index("IsSuperUser", Name = "IX_DocuScanUser_IsSuperUser")]
[Index("LastLogon", Name = "IX_DocuScanUser_LastLogon")]
[Index("AccountName", Name = "UK_DocuScanUser_AccountName", IsUnique = true)]
[Index("UserIdentifier", Name = "UK_DocuScanUser_UserIdentifier", IsUnique = true)]
public partial class DocuScanUser
{
    [Key]
    public int UserId { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string AccountName { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string UserIdentifier { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? LastLogon { get; set; }

    public bool IsSuperUser { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
