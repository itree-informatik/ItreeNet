using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models.DB;

[Table("T_Frontendtest")]
public partial class TFrontendtest
{
    [Key]
    public Guid Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDatum { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDatum { get; set; }

    [StringLength(255)]
    public string Ergebnis { get; set; } = null!;

    public int Total { get; set; }

    public int Korrekt { get; set; }

    public int Fehlerhaft { get; set; }

    [StringLength(255)]
    public string? LastCommit { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<TFrontendtestDetail> TFrontendtestDetail { get; set; } = new List<TFrontendtestDetail>();
}
