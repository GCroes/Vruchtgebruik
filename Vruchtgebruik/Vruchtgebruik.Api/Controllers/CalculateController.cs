using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vruchtgebruik.Api.Interfaces;
using Vruchtgebruik.Api.Models;

namespace Vruchtgebruik.Api.Controllers
{
    /// <summary>
    /// API controller for calculating usage value based on Dutch tax factor methods.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CalculateController : ControllerBase
    {
        private readonly IFactorCalculationMethodFactory _factory;
        private readonly ICorrelationContext _correlationContext;
        private readonly IValidator<CalculationRequest> _validator;
        private readonly ILogger<CalculateController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateController"/> class.
        /// </summary>
        /// <param name="factory">Factory for resolving calculation method strategies.</param>
        /// <param name="correlationContext">Context for accessing correlation ID for the request.</param>
        /// <param name="validator">FluentValidation validator for input request validation.</param>
        /// <param name="logger">Logger instance for diagnostic logging.</param>
        public CalculateController(IFactorCalculationMethodFactory factory, ICorrelationContext correlationContext,
            IValidator<CalculationRequest> validator, ILogger<CalculateController> logger)
        {
            _factory = factory;
            _correlationContext = correlationContext;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the usage value for an asset, based on the selected Dutch tax factor method.
        /// </summary>
        /// <param name="req">Calculation input data, including asset value, age, sex, and factor method.</param>
        /// <returns>
        /// <para>
        /// 200: Calculation successful, returns correlationId and calculation response.<br/>
        /// 400: Validation or calculation error, returns problem details with error information.<br/>
        /// 500: Unexpected server error, returns problem details.
        /// </para>
        /// </returns>
        /// <remarks>
        /// Factor method determines the ruleset used for calculation. Validation and error responses follow RFC7807 (Problem Details).
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculationApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> Calculate([FromBody] CalculationRequest req)
        {
            var correlationId = _correlationContext.CorrelationId;

            _logger.LogInformation(
                "CorrelationId:{CorrelationId} - Calculation requested: Method={FactorMethod}, AssetValue={AssetValue}, Age={Age}, Sex={Sex}",
                correlationId, req.FactorMethod, req.AssetValue, req.Age, req.Sex);

            var validationResult = await _validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return ValidationProblemResult(validationResult, req, correlationId);

            try
            {
                var strategy = _factory.GetStrategy(req.FactorMethod, correlationId);
                var response = strategy.Calculate(req, correlationId);

                _logger.LogInformation(
                    "CorrelationId:{CorrelationId} - Calculation successful: Method={FactorMethod}, UsageValue={UsageValue}, UsedFactor={UsedFactor}",
                    correlationId, req.FactorMethod, response.UsageValue, response.UsedFactor);

                return Ok(new { correlationId, response });
            }
            catch (ArgumentException ex)
            {
                return ArgumentProblemResult(ex, req, correlationId);
            }
            catch (Exception ex)
            {
                return UnexpectedProblemResult(ex, req, correlationId);
            }
        }


#if DEBUG
        /// <summary>
        ///     Triggers an unhandled exception to verify global error handling.
        ///     
        ///     Note: This endpoint exists for integration testing of the API exception handler only.
        ///     It should NOT be exposed in production.
        /// </summary>
        /// <remarks>
        ///     Used by automated integration tests to confirm that the global exception handler
        ///     returns a ProblemDetails response with status code 500.
        /// </remarks>
        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new InvalidOperationException("This is a test exception for integration testing.");
        }
#endif


        /// <summary>
        /// Creates a Bad Request (<c>400</c>) <see cref="ValidationProblemDetails"/> response for invalid input,
        /// logs the validation errors, and includes the correlation and trace IDs.
        /// </summary>
        /// <param name="validationResult">The result of the FluentValidation validation process.</param>
        /// <param name="req">The original calculation request payload.</param>
        /// <param name="correlationId">The correlation ID associated with this request.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the validation problem details and appropriate status code.
        /// </returns>
        /// <returns></returns>
        private IActionResult ValidationProblemResult(FluentValidation.Results.ValidationResult validationResult,
            CalculationRequest req, Guid correlationId)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            _logger.LogWarning(
                "CorrelationId:{CorrelationId} - Validation failed for CalculationRequest: {@Errors} | Payload: {@Request}",
                correlationId,
                errors,
                req
            );

            var pd = new ValidationProblemDetails(errors)
            {
                Title = "Validation failed for the request.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            };
            pd.Extensions["correlationId"] = correlationId.ToString();
            pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return BadRequest(pd);
        }

        /// <summary>
        /// Creates a Bad Request (<c>400</c>) <see cref="ProblemDetails"/> response for argument-related errors
        /// (such as missing factors or invalid input), logs the error, and includes the correlation and trace IDs.
        /// </summary>
        /// <param name="ex">The <see cref="ArgumentException"/> thrown by the business logic.</param>
        /// <param name="req">The original calculation request payload.</param>
        /// <param name="correlationId">The correlation ID associated with this request.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the problem details and appropriate status code.
        /// </returns>
        private IActionResult ArgumentProblemResult(ArgumentException ex, CalculationRequest req, Guid correlationId)
        {
            _logger.LogWarning(ex,
                "CorrelationId:{CorrelationId} - Calculation failed: Method={FactorMethod}, Error={ErrorMessage}",
                correlationId, req.FactorMethod, ex.Message);

            var pd = new ProblemDetails
            {
                Title = "Calculation error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            };
            pd.Extensions["correlationId"] = correlationId.ToString();
            pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return BadRequest(pd);
        }

        /// <summary>
        /// Creates an Internal Server Error (<c>500</c>) <see cref="ProblemDetails"/> response for unexpected or unhandled exceptions,
        /// logs the error, and includes the correlation and trace IDs.
        /// </summary>
        /// <param name="ex">The unhandled <see cref="Exception"/>.</param>
        /// <param name="req">The original calculation request payload.</param>
        /// <param name="correlationId">The correlation ID associated with this request.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the problem details and appropriate status code.
        /// </returns>
        private IActionResult UnexpectedProblemResult(Exception ex, CalculationRequest req, Guid correlationId)
        {
            _logger.LogError(ex,
                "CorrelationId:{CorrelationId} - Unexpected error in calculation: Method={FactorMethod}",
                correlationId, req.FactorMethod);

            var pd = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = HttpContext.Request.Path
            };
            pd.Extensions["correlationId"] = correlationId.ToString();
            pd.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return StatusCode(500, pd);
        }
    }
}
