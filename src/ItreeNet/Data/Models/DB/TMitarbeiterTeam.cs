using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_MitarbeiterTeam")]
public partial class TMitarbeiterTeam
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    public Guid TeamId { get; set; }

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TMitarbeiterTeam")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;

    [ForeignKey("TeamId")]
    [InverseProperty("TMitarbeiterTeam")]
    public virtual TTeam Team { get; set; } = null!;
}
