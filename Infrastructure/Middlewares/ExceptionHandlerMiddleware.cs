using ExternalServices.Gateway.Api.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api.Infrastructure.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        #region Fields
        /// <summary>
        ///  Defines the RequestDelegate.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Defines the ILogger<ExceptionHandlerMiddleware>.
        /// </summary>
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        #endregion

        #region Constructor
        /// <summary>
        /// ExceptionHandlerMiddleware
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger<ExceptionHandlerMiddleware>"/>.</param>
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// HandleExceptionAsync
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/>.</param>
        /// <param name="ex">The ex<see cref="Exception"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            string result;

            if (ex is DomainException e)
            {
                var problemDetails = new CustomValidationProblemDetails(new List<ValidationError> { new() { Code = e.Code, Message = e.Message } })
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "One or more validation errors occurred.",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = context.Request.Path,
                };
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(problemDetails);
            }
            else
            {
                _logger.LogError(ex, $"An unhandled exception has occurred, {ex.Message}");
                var problemDetails = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Internal Server Error.",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Instance = context.Request.Path,
                    Detail = "Internal server error occurred!"
                };
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                result = JsonSerializer.Serialize(problemDetails);
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
        }
        #endregion
    }
}
