using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.Gateway.Api.Infrastructure.Middlewares
{
    /// <summary>
    /// Defines the <see cref="RequestResponseLoggingMiddleware" />.
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        #region Fields

        /// <summary>
        /// Defines the _next.
        /// </summary>
        private readonly RequestDelegate _next;

        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/>.</param>
        public RequestResponseLoggingMiddleware(RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The Invoke.
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/>.</param>
        /// <param name="tnLogService">The tnLogService<see cref="ITnLogService"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context.Request);

            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                //Log the response from the server
                await LogResponse(context.Response);

                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        /// <summary>
        /// The LogRequest.
        /// </summary>
        /// <param name="request">The request<see cref="HttpRequest"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task LogRequest(HttpRequest request)
        {
            var body = request.Body;

            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableBuffering();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //...Then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            request.Body = body;

            _logger.LogInformation("Http Request Information: {Environment} Schema:{Scheme} " +
                "Host: {Host} Path: {Path} QueryString: {QueryString} Request Body: {@RequestBody}",
                Environment.NewLine, request.Scheme, request.Host, request.Path, request.QueryString, bodyAsText);
        }

        /// <summary>
        /// The LogResponse.
        /// </summary>
        /// <param name="response">The response<see cref="HttpResponse"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task LogResponse(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            string text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("Http Response Information:{NewLine} StatusCode:{StatusCode} " +
                "Response Body: {@ResponseBody}", Environment.NewLine, response.StatusCode, text);
        }

        #endregion
    }
}
