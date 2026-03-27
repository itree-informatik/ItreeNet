using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class ReleaseValidator : AbstractValidator<Release>
    {
        public ReleaseValidator()
        {
            RuleFor(b => b.ApplikationId).NotNull().WithMessage("Bitte eine Applikation eintragen.");
            RuleFor(b => b.Bezeichnung).NotNull().WithMessage("Bitte eine Bezeichnung eintragen.");
            RuleFor(b => b.Datum).NotNull().WithMessage("Bitte ein Datum eintragen.");
        }
    }
}
