using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class ProjektValidator : AbstractValidator<Projekt>
    {
        public ProjektValidator()
        {
            RuleFor(b => b.Nummer).NotNull().WithMessage("Bitte eine Nummer eintragen.");
            RuleFor(b => b.KundeId).NotNull().WithMessage("Bitte einen Kunden auswählen.");
            RuleFor(b => b.Bezeichnung).NotNull().WithMessage("Bitte eine Bezeichnung eintragen.");
        }
    }
}
