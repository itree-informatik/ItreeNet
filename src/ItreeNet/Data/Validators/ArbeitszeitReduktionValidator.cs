using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class ArbeitszeitReduktionValidator : AbstractValidator<ArbeitszeitReduktion>
    {
        public ArbeitszeitReduktionValidator()
        {
            RuleFor(b => b.Datum).NotNull().WithMessage("Bitte ein Datum angeben.");
            RuleFor(b => b.Reduktion).NotNull().WithMessage("Bitte eine Reduktion eintragen.")
                .GreaterThan(0).WithMessage("Reduktion muss über 0 sein.");
        }
    }
}
