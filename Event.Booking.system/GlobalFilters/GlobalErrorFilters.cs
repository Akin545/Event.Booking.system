using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Diagnostics;
using System.Net;

namespace Event.Booking.system.Filters
{
    public class GlobalErrorFilters : IExceptionFilter
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment env;

        public GlobalErrorFilters(ILogger<GlobalErrorFilters> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this.env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var tracedId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

            if (!context.ExceptionHandled)
            {
                var exception = context.Exception;
                int statusCode;

                switch (true)
                {
                    case bool _ when exception is UnauthorizedAccessException:
                        statusCode = (int)HttpStatusCode.Unauthorized;
                        break;

                    case bool _ when exception is InvalidOperationException:
                        statusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    default:
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.LogError($"GlobalErrorFilters: Error in {context.ActionDescriptor.DisplayName}. {exception.Message}. Stack Trace: {exception.StackTrace}");

                if (env.EnvironmentName.Equals("Production"))
                {
                    var json = new
                    {
                        tracedId,
                        message = "An error occurred"
                    };

                    context.Result = new ObjectResult(json) { StatusCode = statusCode };
                }

                context.Result = new ObjectResult(exception.Message) { StatusCode = statusCode };
                context.ExceptionHandled = true;
            }
        }
    }
}