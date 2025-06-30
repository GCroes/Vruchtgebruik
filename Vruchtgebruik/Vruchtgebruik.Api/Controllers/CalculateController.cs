using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vruchtgebruik.Api.Helpers;
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
        /// <summary>
        /// Performs the calculation based on input.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculationApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        public async Task<ActionResult<CalculationApiResponse>> Calculate([FromBody] CalculationRequest req)
        {
            var correlationId = _correlationContext.CorrelationId;

            var validationResult = await _validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return ValidationProblemResult(validationResult, req, correlationId);

            try
            {
                var strategy = _factory.GetMethod(req.FactorMethod, correlationId);
                var response = strategy.Calculate(req, correlationId);

                return Ok(new CalculationApiResponse(correlationId.ToString(), response));
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
        /// Generates a Bad Request (400) <see cref="ValidationProblemDetails"/> response for failed model validation,
        /// including grouped errors, correlation/trace IDs, and logs the issue.
        /// </summary>
        /// <param name="validationResult">The FluentValidation result containing validation errors.</param>
        /// <param name="req">The calculation request that failed validation.</param>
        /// <param name="correlationId">The correlation ID for tracing this request through the system.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="ValidationProblemDetails"/> response with status 400.
        /// </returns>
        private ActionResult<CalculationApiResponse> ValidationProblemResult(FluentValidation.Results.ValidationResult validationResult,
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
                correlationId, errors, req);

            var pd = ProblemDetailsHelper.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "Validation failed for the request.",
                errorCode: "VAL-001",
                errors: errors,
                correlationId: correlationId.ToString()
            );

            return BadRequest(pd);
        }

        /// <summary>
        /// Generates a Bad Request (400) <see cref="ProblemDetails"/> response for an <see cref="ArgumentException"/>
        /// thrown during calculation, logging the error and including correlation/trace IDs.
        /// </summary>
        /// <param name="ex">The <see cref="ArgumentException"/> thrown by the business logic.</param>
        /// <param name="req">The calculation request that caused the error.</param>
        /// <param name="correlationId">The correlation ID for this request.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="ProblemDetails"/> response with status 400.
        /// </returns>
        private ActionResult<CalculationApiResponse> ArgumentProblemResult(ArgumentException ex, CalculationRequest req, Guid correlationId)
        {
            _logger.LogWarning(ex,
                "CorrelationId:{CorrelationId} - Calculation failed: Method={FactorMethod}, Error={ErrorMessage}",
                correlationId, req.FactorMethod, ex.Message);

            var pd = ProblemDetailsHelper.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest,
                "Calculation error",
                detail: ex.Message,
                errorCode: "CALC-001",
                correlationId: correlationId.ToString()
            );

            return BadRequest(pd);
        }

        /// <summary>
        /// Generates an Internal Server Error (500) <see cref="ProblemDetails"/> response for unexpected/unhandled exceptions,
        /// logging the exception and including correlation/trace IDs for diagnostics.
        /// </summary>
        /// <param name="ex">The unexpected <see cref="Exception"/> that occurred during processing.</param>
        /// <param name="req">The calculation request being processed when the exception occurred.</param>
        /// <param name="correlationId">The correlation ID for this request.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="ProblemDetails"/> response with status 500.
        /// </returns>
        private ActionResult<CalculationApiResponse> UnexpectedProblemResult(Exception ex, CalculationRequest req, Guid correlationId)
        {
            _logger.LogError(ex,
                "CorrelationId:{CorrelationId} - Unexpected error in calculation: Method={FactorMethod}",
                correlationId, req.FactorMethod);

            var pd = ProblemDetailsHelper.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                errorCode: "GEN-500",
                correlationId: correlationId.ToString()
            );

            return StatusCode(500, pd);
        }
    }
}
