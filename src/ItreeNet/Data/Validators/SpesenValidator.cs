using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class SpesenValidator : AbstractValidator<Spesen>
    {
        public SpesenValidator()
        {
            RuleFor(b => b.Datum).NotNull().WithMessage("Bitte ein Datum eintragen.");
            RuleFor(b => b.Betrag).NotNull().WithMessage("Bitte einen Betrag eingeben").GreaterThan(0).WithMessage("Betrag muss grösser als 0 sein.");
            RuleFor(b => b.Spesenart).NotNull().WithMessage("Bitte eine Spesenart eintragen");
            RuleFor(b => b.AnlassOrt).NotNull().WithMessage("Bitte eine Spesenart eintragen");
        }
    }
}
