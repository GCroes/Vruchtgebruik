using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculateController : ControllerBase
    {
        private readonly IFactorCalculationMethodFactory _factory;
        private readonly ICorrelationContext _correlationContext;
        private readonly IValidator<CalculationRequest> _validator;
        private readonly ILogger<CalculateController> _logger;

        public CalculateController(IFactorCalculationMethodFactory factory, ICorrelationContext correlationContext,
            IValidator<CalculationRequest> validator, ILogger<CalculateController> logger)
        {
            _factory = factory;
            _correlationContext = correlationContext;
            _validator = validator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> Calculate([FromBody] CalculationRequest req)
        {
            var correlationId = _correlationContext.CorrelationId;

            _logger.LogInformation("CorrelationId:{CorrelationId} - Calculation requested: Method={FactorMethod}, AssetValue={AssetValue}, Age={Age}, Sex={Sex}",
                correlationId, req.FactorMethod, req.AssetValue, req.Age, req.Sex);

            var validationResult = await _validator.ValidateAsync(req);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList();

                _logger.LogWarning(
                    "CorrelationId:{CorrelationId} - Validation failed for CalculationRequest: {@Errors} | Payload: {@Request}",
                    _correlationContext.CorrelationId,
                    errors,
                    req
                );

                return BadRequest(new { correlationId = _correlationContext.CorrelationId, errors });
            }

            try
            {
                var strategy = _factory.GetStrategy(req.FactorMethod, correlationId);
                var response = strategy.Calculate(req, correlationId);

                _logger.LogInformation("CorrelationId:{CorrelationId} - Calculation successful: Method={FactorMethod}, UsageValue={UsageValue}, UsedFactor={UsedFactor}",
                    correlationId, req.FactorMethod, response.UsageValue, response.UsedFactor);

                return Ok(new { correlationId, response });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "CorrelationId:{CorrelationId} - Calculation failed: Method={FactorMethod}, Error={ErrorMessage}",
                    correlationId, req.FactorMethod, ex.Message);

                return BadRequest(new { correlationId, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CorrelationId:{CorrelationId} - Unexpected error in calculation: Method={FactorMethod}",
                    correlationId, req.FactorMethod);

                return StatusCode(500, new { correlationId, error = "An unexpected error occurred." });
            }
        }


    }

}
