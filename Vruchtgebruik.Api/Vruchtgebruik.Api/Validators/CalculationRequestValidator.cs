using FluentValidation;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Validators
{
    public class CalculationRequestValidator : AbstractValidator<CalculationRequest>
    {
        public CalculationRequestValidator()
        {
            RuleFor(x => x.AssetValue).GreaterThan(0);
            RuleFor(x => x.Age).InclusiveBetween(0, 130);
            RuleFor(x => x.Sex)
                .NotEmpty().WithMessage("Sex is required.")
                .Must(x => x == "male" || x == "female")
                .WithMessage("Sex must be 'male' or 'female'.");
            RuleFor(x => x.FactorMethod).NotEmpty();
        }
    }

}
