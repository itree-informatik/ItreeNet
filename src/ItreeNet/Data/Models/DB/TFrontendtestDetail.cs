using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models.DB;

[Table("T_FrontendtestDetail")]
public partial class TFrontendtestDetail
{
    [Key]
    public Guid Id { get; set; }

    public Guid FrontendtestId { get; set; }

    [StringLength(255)]
    public string TestName { get; set; } = null!;

    [StringLength(255)]
    public string Dauer { get; set; } = null!;

    [StringLength(255)]
    public string Ergebnis { get; set; } = null!;

    public string? StdOut { get; set; }

    public string? ErrMessage { get; set; }

    public string? StackTrace { get; set; }

    [ForeignKey("FrontendtestId")]
    [InverseProperty("TFrontendtestDetail")]
    public virtual TFrontendtest Parent { get; set; } = null!;

    [InverseProperty("Parent")]
    public virtual ICollection<TFrontendtestBild> TFrontendtestBild { get; set; } = new List<TFrontendtestBild>();
}
