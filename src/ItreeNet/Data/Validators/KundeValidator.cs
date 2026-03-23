using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class KundeValidator : AbstractValidator<Kunde>
    {
        public KundeValidator()
        {
            RuleFor(b => b.Kundenname).NotNull().WithMessage("Bitte einen Kundennamen eintragen.");
            //RuleFor(b => b.Adresse).NotNull().WithMessage("Bitte eine Adresse eintragen.");
        }
    }
}
