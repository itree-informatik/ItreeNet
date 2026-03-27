using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class ApplikationValidator : AbstractValidator<Applikation>
    {
        public ApplikationValidator()
        {
            RuleFor(b => b.Bezeichnung).NotNull().WithMessage("Bitte eine Bezeichnung eintragen.");
        }
    }
}
