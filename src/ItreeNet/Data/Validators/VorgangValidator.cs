using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class VorgangValidator : AbstractValidator<Vorgang>
    {
        public VorgangValidator()
        {
            RuleFor(b => b.ProjektId).NotNull().WithMessage("Bitte ein Projekt auswählen.");
            RuleFor(b => b.Bezeichnung).NotNull().WithMessage("Bitte eine Bezeichnung eintragen.");
        }
    }
}
