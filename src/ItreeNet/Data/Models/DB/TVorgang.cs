using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Vorgang")]
public partial class TVorgang
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProjektId { get; set; }

    [StringLength(100)]
    public string Bezeichnung { get; set; } = null!;

    public bool Aktiv { get; set; }

    public bool Ferien { get; set; }

    public bool Gleitzeit { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Stundenansatz { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal AnzahlStunden { get; set; }

    [ForeignKey("ProjektId")]
    [InverseProperty("TVorgang")]
    public virtual TProjekt Projekt { get; set; } = null!;

    [InverseProperty("OriginalVorgang")]
    public virtual ICollection<TBuchung> TBuchungOriginalVorgang { get; set; } = new List<TBuchung>();

    [InverseProperty("Vorgang")]
    public virtual ICollection<TBuchung> TBuchungVorgang { get; set; } = new List<TBuchung>();

}
