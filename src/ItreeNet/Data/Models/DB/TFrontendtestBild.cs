using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models.DB;

[Table("T_FrontendtestBild")]
public partial class TFrontendtestBild
{
    [Key]
    public Guid Id { get; set; }

    public Guid FrontendtestDetailId { get; set; }

    [StringLength(4000)]
    public string Verzeichnis { get; set; } = null!;

    public byte[] Bild { get; set; } = null!;

    [ForeignKey("FrontendtestDetailId")]
    [InverseProperty("TFrontendtestBild")]
    public virtual TFrontendtestDetail Parent { get; set; } = null!;
}
