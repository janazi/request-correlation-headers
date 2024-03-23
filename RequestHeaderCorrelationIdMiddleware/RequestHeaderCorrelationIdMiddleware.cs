using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Jnz.RequestHeaderCorrelationId
{
    public class RequestHeaderCorrelationIdMiddleware(RequestDelegate next)
    {
        private const string CorrelationTokenHeader = "x-correlation-id";
        public RequestDelegate Next { get; } = next;
        public ILogger<RequestHeaderCorrelationIdMiddleware> Logger { get; }


        public async Task InvokeAsync(HttpContext context)
        {
            if (!(!StringValues.IsNullOrEmpty(context.Request.Headers[CorrelationTokenHeader])
                && Guid.TryParse(context.Request.Headers[CorrelationTokenHeader], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
                context.Request.Headers.Add(CorrelationTokenHeader, correlationId.ToString());
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
