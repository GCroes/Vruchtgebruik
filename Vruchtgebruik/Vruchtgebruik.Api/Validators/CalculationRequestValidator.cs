using FluentValidation;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Validators
{
    /// <summary>
    /// FluentValidation validator for <see cref="CalculationRequest"/>.
    /// Defines validation rules for asset value, age, sex, and factor method.
    /// </summary>
    public class CalculationRequestValidator : AbstractValidator<CalculationRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRequestValidator"/> class.
        /// Sets up all validation rules for the <see cref="CalculationRequest"/> model.
        /// </summary>
        public CalculationRequestValidator()
        {
            // Asset value must be greater than zero.
            RuleFor(x => x.AssetValue).GreaterThan(0);

            // Age must be between 0 and 130 (inclusive).
            RuleFor(x => x.Age).InclusiveBetween(0, 130);

            // Sex is required and must be either "man" or "vrouw".
            RuleFor(x => x.Sex)
                .NotEmpty().WithMessage("Geslacht is verplicht.")
                .Must(x => x == "male" || x == "female")
                .WithMessage("Geslacht moet 'man' of 'vrouw' zijn.");

            // Factor method is required (not empty).
            RuleFor(x => x.FactorMethod).NotEmpty();
        }
    }

}
