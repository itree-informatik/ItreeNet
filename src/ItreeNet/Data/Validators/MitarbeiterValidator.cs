using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class MitarbeiterValidator : AbstractValidator<Mitarbeiter>
    {
        public MitarbeiterValidator()
        {
            RuleFor(b => b.Vorname).NotEmpty().WithMessage("Bitte einen Nachnamen eintragen.");
            RuleFor(b => b.Nachname).NotEmpty().WithMessage("Bitte einen Vornamen eintragen.");
            RuleFor(b => b.Email).EmailAddress().WithMessage("Kein gültiges Emailformat.");
            RuleFor(b => b.Eintritt).NotNull().WithMessage("Bitte einen gültigen Eintritt angeben.");
            //RuleFor(b => b.AzureId).NotEmpty().WithMessage("Bitte eine AzureId eintragen.").Must(ValidateGuid).WithMessage("AzureId ist keine valide Guid.");
        }

        private bool ValidateGuid(string? guidString)
        {
            return Guid.TryParse(guidString, out _);
        }
    }
}
