using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Team")]
public partial class TTeam
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(50)]
    public string Bezeichnung { get; set; } = null!;

    [StringLength(50)]
    public string NaturalId { get; set; } = null!;

    public byte Sort { get; set; }

    public bool Aktiv { get; set; }

    [InverseProperty("Team")]
    public virtual ICollection<TKunde> TKunde { get; set; } = new List<TKunde>();

    [InverseProperty("Team")]
    public virtual ICollection<TMitarbeiterTeam> TMitarbeiterTeam { get; set; } = new List<TMitarbeiterTeam>();
}
