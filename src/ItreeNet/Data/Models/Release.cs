namespace ItreeNet.Data.Models;

public partial class Release
{
    public Guid Id { get; set; }

    public Guid ApplikationId { get; set; }

    public DateOnly Datum { get; set; }

    public string Bezeichnung { get; set; } = null!;

    public string ApplikationName {get; set; } = null!;
}
