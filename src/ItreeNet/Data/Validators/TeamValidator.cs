using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class TeamValidator : AbstractValidator<Team>
    {
        public TeamValidator()
        {
            RuleFor(b => b.Bezeichnung).NotNull().WithMessage("Bitte eine Bezeichnung eintragen.");
            RuleFor(b => b.Sort)
                .NotNull().WithMessage("Bitte eine Sortierung eintragen.");
            RuleFor(b => b.NaturalId)
                .Length(1, 50).WithMessage("Die Länge muss zwischen 1 und 50 sein.")
                .NotNull().WithMessage("Bitte eine NaturalId eintragen.");
        }
    }
}
