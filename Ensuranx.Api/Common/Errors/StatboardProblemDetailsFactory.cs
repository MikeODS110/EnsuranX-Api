using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Ensuranx.Api.Common.Errors
{
    public class EnsuranxProblemDetailsFactory : ProblemDetailsFactory
    {
        private readonly ApiBehaviorOptions _options;
        private readonly ILogger<EnsuranxProblemDetailsFactory> _logger;

        public EnsuranxProblemDetailsFactory(IOptions<ApiBehaviorOptions> options, ILogger<EnsuranxProblemDetailsFactory> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }


        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            statusCode ??= 500;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Type = type,
                Detail = detail,
                Instance = instance
            };

            if (title != null)
            {
                problemDetails.Title = title;
            }

            ApplyProblemDetailsDefaults(httpContext,problemDetails,statusCode.Value);

            return problemDetails;
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
        {
            ValidationProblemDetails validationProblemDetails = new ValidationProblemDetails(modelStateDictionary);
            statusCode ??= 400;
            if (_options.ClientErrorMapping.TryGetValue(statusCode.Value, out var clientErrorData))
            {
                validationProblemDetails.Title ??= clientErrorData.Title;
                validationProblemDetails.Type ??= clientErrorData.Link;
            }
            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                validationProblemDetails.Extensions["traceId"] = traceId;
            }
            validationProblemDetails.Status ??= statusCode;
            _logger.LogError("validationProblemDetails {@validationProblemDetails}", validationProblemDetails);
            return validationProblemDetails;
        }

        private void ApplyProblemDetailsDefaults(HttpContext httpContext,ProblemDetails problemDetails,int statusCode)
        {
            problemDetails.Status ??= statusCode;

            if (_options.ClientErrorMapping.TryGetValue(statusCode,out var clientErrorData))
            {
                problemDetails.Title ??= clientErrorData.Title;
                problemDetails.Type ??= clientErrorData.Link;
            }

            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                problemDetails.Extensions["traceId"] = traceId;
            }
            //var errors = httpContext?.Items["errors"] as List<Error>;
            //problemDetails.Extensions.Add("errorCodes", errors.Select(e => e.Code));
        }
    }
}
