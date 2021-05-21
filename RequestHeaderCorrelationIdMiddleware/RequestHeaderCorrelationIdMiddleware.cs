using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace CorrelationIdRequestHeader
{
    public class RequestHeaderCorrelationIdMiddleware
    {
        public RequestHeaderCorrelationIdMiddleware(RequestDelegate next)
        {
            Next = next;
        }
        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";
        public RequestDelegate Next { get; }
        public ILogger<RequestHeaderCorrelationIdMiddleware> Logger { get; }


        public async Task InvokeAsync(HttpContext context)
        {
            if (!(!StringValues.IsNullOrEmpty(context.Request.Headers[CORRELATION_TOKEN_HEADER])
                && Guid.TryParse(context.Request.Headers[CORRELATION_TOKEN_HEADER], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
                context.Request.Headers.Add(CORRELATION_TOKEN_HEADER, correlationId.ToString());
            }

            await Next(context);
        }
    }

    public static class RequestHeadersCorrealationIdMiddlewareExtensions
    {
        public static IApplicationBuilder AddRequestHeaderCorrelationId(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<RequestHeaderCorrelationIdMiddleware>();
        }
    }
}
